﻿/// <reference path="~/js/openiz-model.js"/>
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
angular.module('layout').controller('AuthenticationDialogController', ['$scope', function ($scope) {

    $scope.authenticate = function (form) {
        if(!form.$valid) 
        {
            console.log(OpenIZ.Localization.getString('locale.error.login.invalid'));
            return;
        }

        // Do an authentication that is not session binding
        OpenIZ.App.showWait('#loginButton');
        OpenIZ.Authentication.loginAsync({
            userName: $scope.username,
            password: $scope.password,
            scope: "elevation",
            continueWith: function(session) {
                if (session == null) {
                    console.log(OpenIZ.Localization.getString("err_oauth2_invalid_grant"));
                }
                else {
                    OpenIZ.Authentication.$elevationCredentials = {
                        userName : $scope.username,
                        password : $scope.password,
                        $enabled : true,
                        continueWith: OpenIZ.Authentication.$elevationCredentials.continueWith
                    };
                    OpenIZ.Authentication.hideElevationDialog();
                    $scope.userName = null; $scope.password = null;
                    if (OpenIZ.Authentication.$elevationCredentials.continueWith != null) {
                        OpenIZ.Authentication.$elevationCredentials.continueWith();
                        OpenIZ.Authentication.$elevationCredentials = {};
                    }
                }
            },
            onException: function(error) {
                OpenIZ.App.hideWait('#loginButton');
                if (error.error != null)
                    alert(OpenIZ.Localization.getString(error.error));
                else if(error.message != null)
                    alert(OpenIZ.Localization.getString(error.message));
                else
                    console.log(error);
            }
        })
    }
    
}]);