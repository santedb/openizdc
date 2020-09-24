/// <reference path="~/js/openiz.js"/>
/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2017-10-30
 */

/// <reference path="~/js/openiz-model.js"/>
/// <reference path="~/lib/jquery.min.js"/>
/// <reference path="~/lib/angular.min.js"/>

angular.module('layout').controller('SyncCentreController', ['$scope', '$state', '$rootScope', function ($scope, $state, $rootScope) {

    var converter = new showdown.Converter();

    $scope.queue = {};
    $scope.closeQueue = closeQueue;
    $scope.requeueAllDead = requeueAllDead;
    $scope.renderB64 = renderB64;
    $scope.requeueItem = requeueItem;
    $scope.deleteQueueItem = deleteQueueItem;
    $scope.selectItem = selectItem;
    $scope.forceSync = forceSync;

    $scope.$watch('queue.current', function (n, o) {
        if (n && n != o) {
            n.CollectionItem = null;
            getQueue(n.name);
        }
    });


    function forceSync() {

        OpenIZ.App.showWait("#forceSync");
        OpenIZ.Queue.forceResyncAsync({
            onException: function(ex) {
                OpenIZ.App.showErrorDialog(ex);
            },
            finally: function () {
                OpenIZ.App.hideWait("#forceSync");
            }
        });
    }

    function getQueue(queueName) {
        OpenIZ.Queue.getQueueAsync({
            query: { _count: 20, id: "!null" },
            queueName: queueName,
            continueWith: function (data) {
                $scope.queue[queueName].name = queueName;
                if ($scope.queue[queueName]) {
                    $scope.queue[queueName].Size = data.Size;
                    $scope.queue[queueName].CollectionItem = data.CollectionItem;
                }
                else
                    $scope.queue[queueName] = data;
                $scope.queue[queueName].name = queueName;
                $scope.$apply();
            },
            onException: function (data) {
                $scope.queue[queueName] = { Size: 0 };
                $scope.$apply();
            }
        });
    }

    function getQueueCount(queueName) {
        OpenIZ.Queue.getQueueAsync({
            queueName: queueName,
            query: { _count: 0, id: "!null" },
            continueWith: function (data) {
                if ($scope.queue[queueName]) {
                    $scope.queue[queueName].Size = data.Size;
                }
                else
                    $scope.queue[queueName] = data;
                $scope.queue[queueName].name = queueName;
                $scope.$apply();
            },
            onException: function (data) {
                $scope.queue[queueName] = { Size: 0 };
                $scope.$apply();
            }
        });
    }

    function closeQueue() {
        console.log("close");
        $scope.queue.current = null;
        delete $scope.queue.current;
    }

    function selectItem(id) {
        $scope.isLoading = true;
        OpenIZ.Queue.getQueueAsync({
            queueName: $scope.queue.current.name,
            id: id,
            continueWith: function (data) {
                $scope.queue.currentItem = data.CollectionItem[0];
                $scope.queue.currentItem.payload = renderB64($scope.queue.currentItem.payload);
                if ($scope.queue.currentItem.kb) {
                    $scope.queue.currentItem.kb.resolution = converter.makeHtml($scope.queue.currentItem.kb.resolution);
                }
                if ($scope.queue.currentItem.exception && $scope.queue.currentItem.exception.length > 1024)
                    $scope.queue.currentItem.exception = $scope.queue.currentItem.exception.substring(0, 1024) + " ... " + ($scope.queue.currentItem.exception.length - 1024) + " more bytes";
                if ($scope.queue.currentItem.payload && $scope.queue.currentItem.payload.length > 1024) 
                    $scope.queue.currentItem.payload = $scope.queue.currentItem.payload.substring(0, 1024) + " ... " + ($scope.queue.currentItem.payload.length - 1024) + " more bytes";

            },
            onException: function (ex) {
                OpenIZ.App.showErrorDialog(ex);
            },
            finally: function () {
                try {
                    $scope.isLoading = false;
                    $scope.$apply();
                }
                catch (e) {}
            }
        })
    }

    function requeueAllDead(queueId, acknowledgedUnsafe) {

        if (!acknowledgedUnsafe) {
            if (!confirm(OpenIZ.Localization.getString("locale.sync.batchForceConfirm")))
                return;
            OpenIZ.App.showWait("#reQueueDead")
            OpenIZ.App.showWait("#reQueueDeadModal")
        }

        OpenIZ.Queue.requeueDeadAsync({
            continueWith: function (data, state) {
                // Last queue item?
                refreshQueueState(true);
                OpenIZ.App.hideWait("#reQueueDead");
                OpenIZ.App.hideWait("#reQueueDeadModal");
            },
            onException: function (ex) {
                OpenIZ.App.showErrorDialog(ex);
                OpenIZ.App.hideWait("#reQueueDead");
                OpenIZ.App.hideWait("#reQueueDeadModal");

            }
        });
    };

    
    // Render the specified tag from base64
    function renderB64(tag) {
        if (tag) {
            var tagContent = atob(tag);
            return tagContent;
        }
    }
    
    // Re-queue the specified item so the mobile will attempt to re-send
    // @param {int} itemId The item in the dead-letter queue to be re-queued
    function requeueItem(itemId) {
        if (!confirm(OpenIZ.Localization.getString("locale.sync.forceConfirm")))
            return;

        OpenIZ.App.showWait("#requeQueueItem");
        OpenIZ.Queue.requeueDeadAsync({
            queueId: itemId,
            continueWith: function (e) {
                refreshQueueState(true);
            },
            onException: function (ex) {
                OpenIZ.App.showErrorDialog(ex);
            },
            finally: function () {
                OpenIZ.App.hideWait("#requeQueueItem");
                $("#resolveDialog").modal('hide');
            }
        });
    }

    // Delete the specified item so the mobile will attempt to re-send
    // @param {int} itemId The item in the dead-letter queue to be re-queued
    function deleteQueueItem(itemId) {
        if ($scope.queue.currentItem.operation == "Insert" && !confirm(OpenIZ.Localization.getString("locale.sync.deleteConfirm.insert")))
            return ;
        else if($scope.queue.currentItem.operation != "Insert" && !confirm(OpenIZ.Localization.getString("locale.sync.deleteConfirm.insert")))
            return;

        OpenIZ.App.showWait("#deleteQueueItem");

        var doDelete = function () {
            OpenIZ.Queue.deleteQueueAsync({
                queueId: itemId,
                queueName: OpenIZ.Queue.QueueNames.DeadLetterQueue,
                continueWith: function (e) {
                    refreshQueueState(true);
                },
                onException: function (ex) {
                    if (ex.type == "PolicyViolationException") return; // allow override
                    OpenIZ.App.showErrorDialog(ex);
                },
                finally: function () {
                    OpenIZ.App.hideWait("#deleteQueueItem");
                    $("#resolveDialog").modal('hide');
                }
            });
        };

        OpenIZ.Authentication.$elevationCredentials.continueWith = doDelete;
        doDelete();
    }

    // Refresh queue state
    // @param {bool} noTimer When true, instructs the function not to re-run on a timer
    function refreshQueueState(noTimer) {

        getQueueCount(OpenIZ.Queue.QueueNames.InboundQueue);
        getQueueCount(OpenIZ.Queue.QueueNames.OutboundQueue);
        getQueueCount(OpenIZ.Queue.QueueNames.DeadLetterQueue);
        getQueueCount(OpenIZ.Queue.QueueNames.AdminQueue);

        if (!noTimer && $state.is('org-openiz-core.sync') && $rootScope.session != null)
            setTimeout(refreshQueueState, 10000);
    }

    refreshQueueState();

    OpenIZ.App.getInfoAsync({
        continueWith: function (d) {
            $scope.about = d;
            $scope.$apply();
        }
    });

}]);