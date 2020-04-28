/// <reference path="~/js/openiz-model.js"/>
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

/// <reference path="~/js/openiz.js"/>
/// <reference path="~/lib/angular.min.js"/>
/// <reference path="~/lib/jquery.min.js"/>
/// <reference path="~/lib/bootstrap.min.js"/>

angular.module('layout').controller('LoginPartController', ['$scope', '$window', '$stateParams', '$rootScope', '$templateCache', '$state', '$timeout', function ($scope, $window, $stateParams, $rootScope, $templateCache, $state, $timeout) {
    // Get the current scope that we're in

    $scope.showPasswordReset = $scope.showPasswordReset || function () {
        $scope.loginForm.$setUntouched();
        $('#passwordResetDialog').modal({
            backdrop: 'static',
            keyboard: false
        });
    };

    var getSystemAlerts = function () {
        OpenIZ.App.getAlertsAsync({
            query: { flags: 8 },
            continueWith: function (alerts) {
                try {
                    $rootScope.systemAlerts = alerts;

                    if (alerts.length > 0)
                        for (var i in alerts) {
                            alerts[i].dismiss = function () {
                                OpenIZ.App.saveAlertAsync({ data: { id: this.id, flags: 4, subject: this.subject, body: this.body, from: this.from, to: this.to } });
                                $rootScope.systemAlerts.splice(alerts.indexOf(this), 1);
                            };
                        }
                    else if(getSystemAlerts){
                        $timeout(getSystemAlerts, 1000);
                        getSystemAlerts = null;
                    }
                    $rootScope.$apply();
                }
                catch (e) {
                    console.error(e);
                }
            }
        });
    }
    getSystemAlerts();

    $scope.login = $scope.login || function (form, username, password) {

        if (!form.$valid) {
            console.log(OpenIZ.Localization.getString("locale.security.login.invalid"));
            return;
        }
        OpenIZ.App.showWait('#loginButton');
        OpenIZ.Authentication.loginAsync({
            userName: username,
            password: password,
            continueWith: function (session) {
                if (session == null) {
                    alert(OpenIZ.Localization.getString("err_oauth2_invalid_grant"));
                }
                else {
                    $rootScope.initSessionVars();
                    $templateCache.removeAll();
                    $state.reload();
                    //$window.location.reload();
                    //$state.go($stateParams.redirectUrl, $stateParams.params)

                }
            },
            onException: function (ex) {
                //OpenIZ.App.hideWait('#loginButton');
                OpenIZ.App.hideWait('#loginButton');

                if (typeof (ex) == "string")
                    console.log(ex);
                else if (ex.details && ex.message)
                    alert(OpenIZ.Localization.getString(ex.message) + " - " + OpenIZ.Localization.getString(ex.details));
                else if (ex.message)
                    alert(OpenIZ.Localization.getString(ex.message));
                else
                    console.log(ex);
            },
            finally: function () {
            }
        });
    }; // scope.login


}]);