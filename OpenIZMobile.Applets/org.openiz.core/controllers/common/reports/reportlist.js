﻿/*
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

/// <reference path="~/js/openiz.js"/>

// Report list controller
angular.module('layout').controller('ReportListController', ['$scope', '$rootScope', '$compile', function ($scope, $rootScope, $compile) {

    $scope.isLoading = true;
    $scope.reports = [];
    $scope.executeReport = executeReport;
    $scope.currentReport = {};
    $scope.selectItem = selectItem;
    $scope.currentFilter = {};
    $scope.reportBody = '';
    $scope.viewId = 0;
    $scope.setView = function (viewId) {
        $scope.viewId = viewId;
    };
    $scope.getParameterMaxDate = getParameterMaxDate;
    $scope.getParameterMinDate = getParameterMinDate;
    $scope.resetReportModal = resetReportModal;

    $scope.$watch('reportBody', function (nv, ov) {
        $("#reportBody").html(nv);
        $compile($("#reportBody").contents())($scope);

    });
    $scope.$watch('viewId', function (nv, ov) {
        if(nv != ov && nv != null)
            $scope.executeReport();
    });
    // Get reports asynchronously
    OpenIZ.Risi.getReportsAsync({
        continueWith: function (data) {
            $scope.reports = data;
        },
        onException: function (ex) {
            if (ex.message)
                alert(OpenIZ.Localization.getString(ex.message));
            else
                console.error(ex);
        },
        finally: function () {
            $scope.isLoading = false;
            $scope.$apply();
        }
    });

    // Select item from list
    function selectItem(rpt) {

        $scope.viewId = null;
        $scope.isLoading = true;
        OpenIZ.Risi.getReportsAsync({
            name: rpt.name,
            continueWith: function (data) {
                $scope.currentFilter = {};
                $scope.currentReport = data[0];
                for (var i in data[0].parameter) {
                    $scope.currentFilter[data[0].parameter[i].name] = null;
                    if (data[0].parameter[i].valueSet) {
                        data[0].parameter[i].valueSet.values = [{ 0: null, 1: OpenIZ.Localization.getString('locale.dialog.wait.text') }];
                        OpenIZ.Risi.executeDatasetAsync({
                            name: data[0].parameter[i].name,
                            report: rpt.name,
                            query: { locale: OpenIZ.Localization.getLocale() },
                            continueWith: function (d, state) {
                                data[0].parameter[state].valueSet.values = d;
                                $scope.$apply();
                            },
                            state: i
                        });
                    }
                }
            },
            onException: function (ex) {
                if (ex.message)
                    alert(OpenIZ.Localization.getString(ex.message));
                else
                    console.error(ex);
            },
            finally: function () {
                $scope.isLoading = false;
                $scope.$apply();
            }
        });

    }

    // Execute report
    function executeReport(reportForm) {
        if (reportForm && reportForm.$invalid) {
            return;
        }

        $("#reportResultDialog").modal('show');

        var filters = {};
        // Fix values 
        for (var i in $scope.currentReport.parameter) {
            var parm = $scope.currentReport.parameter[i];
            if (parm.type == "Date") {
                filters[parm.name] = OpenIZ.Util.toDateInputString($scope.currentFilter[parm.name]);
            }
            else
                filters[parm.name] = $scope.currentFilter[parm.name];
        }

        $scope.isLoading = true;
        // TODO: Make this on-demand
        OpenIZ.Risi.executeReportAsync({
            name: $scope.currentReport.info.name,
            view: $scope.currentReport.view[$scope.viewId || 0].name,
            query: filters,
            continueWith: function (data) {
                if (data == null)
                    $scope.reportBody = "<h2>" + OpenIZ.Localization.getString("locale.reports.nodata") + "</h2>";
                else {
                    $scope.reportBody = data;
                }
            },
            onException: function (ex) {
                $scope.reportBody = "<h2>" + OpenIZ.Localization.getString("locale.reports.nodata") + "</h2>";
                alert(OpenIZ.Localization.getString("locale.reports.error.locked"));
            },
            finally: function () {
                $scope.isLoading = false;
                $scope.$apply();
            }
        });
    }

    $rootScope.confirmNavigation = function (event, fromState) {
        var canNavigate = true;

        if (fromState.name == "org-openiz-core.reports") {
            if ($('#reportParametersDialog').hasClass('in')) {
                if ($('#reportResultDialog').hasClass('in')) {
                    $('#reportResultDialog').modal('hide');
                }
                else {
                    $('#reportParametersDialog').modal('hide');
                }

                canNavigate = false;
            }
        }

        return canNavigate;
    }

    /* Gets the maximum date for the parameter */
    function getParameterMaxDate(parameter) {
        var max = parameter.max || $rootScope.page.currentTime;

        if (parameter.name === "DateFrom" && $scope.currentFilter["DateTo"]) {
            max = $scope.currentFilter["DateTo"];
        }

        return max;
    }

    /* Gets the minimum date for the parameter */
    function getParameterMinDate(parameter) {
        var min = parameter.min || 0;

        if (parameter.name === "DateTo" && $scope.currentFilter["DateFrom"]) {
            min = $scope.currentFilter["DateFrom"];
        }

        return min;
    }

    /* Resets the report modal form and errors */
    function resetReportModal(form) {
        currentReport = null;
        form.$setPristine();
        form.$setUntouched();
    }
}]);