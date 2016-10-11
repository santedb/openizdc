﻿/// <reference path="../../js/openiz-model.js"/>

/// <reference path="../../js/openiz.js"/>

layoutApp.controller('SettingsController', ['$scope', function ($scope) {

    OpenIZ.App.showWait();
    OpenIZ.Configuration.getConfigurationAsync({
        continueWith: function (config) {
            $scope.config = config;
            $scope.config.network.useProxy = $scope.config.network.proxyAddress != null;
            $scope.config.network.proxyAddress = $scope.config.network.proxyAddress || null;
            $scope.ui = {
                dataCollapsed: true,
                securityCollapsed: true
            };

            $scope.config.data.mode = "sync"; //OpenIZ.App.getService("SynchronizationManagerService") == null ?
            //OpenIZ.App.getService("ImsiPersistenceService") == null ? "offline" : "online" : "sync";
            $scope.config.data.sync = {
                event: [],
                enablePoll: OpenIZ.App.getService("ImsiPollingService") != null,
                pollInterval: OpenIZ.Configuration.getApplicationSetting("imsi.poll.interval")
            };
            $scope.config.log.mode = $scope.config.log.trace[0].filter || "Warning";
            $scope.config.security.hasher = OpenIZ.App.getService("IPasswordHashingService") || "SHA256PasswordHasher";

            $scope.config.security.offline = {
                enable: false
            };
            $scope.$apply();
        },
        onException : function(error) {
            if(error.message != null)
                alert(error.message);
            else
                alert(error);
        },
        finally: function () {
            OpenIZ.App.hideWait();
        }
    });
    
    
    $scope.master = {};

    // leave realm
    $scope.leaveRealm = function (realm) {
        if (confirm(OpenIZ.Localization.getString("locale.settings.confirm.leaveRealm")))
            OpenIZ.Configuration.leaveRealmAsync();
    };

    // join realm
    $scope.joinRealm = function (realm) {
        OpenIZ.App.showWait();
        OpenIZ.Configuration.joinRealmAsync({
            domain: realm.domain,
            deviceName: realm.deviceName,
            continueWith: function (data) {
                $scope.config.realmName = data.realmName;
                alert(OpenIZ.Localization.getString("locale.settings.status.joinRealm"));
            },
            finally: function () {
                OpenIZ.App.hideWait();
            }
        });
    };

    // Save config
    $scope.save = function (config) {
        
        if ($scope.config.realmName == null)
            alert(OpenIZ.Localization.getString("locale.settings.error.noRealm"));
        else {
            OpenIZ.App.showWait();
            OpenIZ.Configuration.saveAsync({
                data: config,
                continueWith: function () {
                    OpenIZ.App.close();
                }
            });
        }
    };
}]);
