﻿/// <reference path="~/js/openiz.js"/>
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
/// <reference path="~/controllers/layouts/navbar.js"/>

angular.module('layout').controller('ViewAlertController', ['$scope', '$stateParams', '$state', function ($scope, $stateParams, $state)
{
    $scope.selectedMessageID = null;

    $scope.deleteAlert = deleteAlert;
    $scope.updateAlert = updateAlert;
    $scope.closeMessage = closeMessage;

    angular.element(document).ready(init);

    function init() {
        $scope.selectedMessageID = $stateParams.alertId == '' ? null : $stateParams.alertId;

        if ($scope.selectedMessageID !== null)
        {
            OpenIZ.App.getAlertsAsync({
                query: {
                    id: $stateParams.alertId,
                    _count: 1
                },
                onException: function (ex)
                {
                    OpenIZ.App.hideWait();
                    OpenIZ.App.showErrorDialog(ex);

                },
                continueWith: function (data)
                {
                    $scope.alert = data[0];
                    // HACK: remove PRE tags
                    $scope.alert.body = $scope.alert.body.replace("<pre>", "").replace("</pre>", "");
                    var converter = new showdown.Converter();
                    $scope.alert.body = converter.makeHtml($scope.alert.body);
                    $scope.$apply();
                }
            });
        }
    }    

    function deleteAlert(alert)
    {
        alert.flags = 2;

        $scope.alert.creationTime = $scope.alert.time;

        OpenIZ.App.deleteAlertAsync({
            data: alert,
            continueWith: function (data)
            {
                OpenIZ.App.showWait();
                OpenIZ.App.toast(OpenIZ.Localization.getString("locale.alert.updateSuccessful"));
            },
            onException: function (ex)
            {
                OpenIZ.App.showErrorDialog(ex);
                OpenIZ.App.toast(OpenIZ.Localization.getString("locale.alert.updateUnsuccessful"));
            },
            finally: function ()
            {
                OpenIZ.App.hideWait();
            }
        });
    };

    function updateAlert()
    {
        $scope.alert.flags = 2;
        $scope.alert.creationTime = $scope.alert.time;

        OpenIZ.App.saveAlertAsync({
            data: $scope.alert,
            continueWith: function (data)
            {
                OpenIZ.App.showWait();
                OpenIZ.App.toast(OpenIZ.Localization.getString("locale.alert.updateSuccessful"));
                $state.transitionTo($state.current, {alertId: "" }, {
                    reload: true, inherit: false
                });
            },
            onException: function (ex)
            {
                OpenIZ.App.showErrorDialog(ex);
                OpenIZ.App.toast(OpenIZ.Localization.getString("locale.alert.updateUnsuccessful"));
            },
            finally: function ()
            {
                for (var i = 0; i < $scope.messages.length; i++) {
                    if ($scope.messages[i].id == $scope.alert.id) {
                        $scope.messages.splice(i, 1);
                        break;
                    }
                }
                OpenIZ.App.hideWait();
            }
        });
    };

    function closeMessage() {
        $scope.selectedMessageID = null;
        delete $scope.alert;
        $state.go('.', { alertId: undefined });
    }
}]);