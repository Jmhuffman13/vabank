﻿(function () {
    'use strict';

    angular
        .module('vabank.webapp')
        .controller('loginController', loginController);

    loginController.$inject = ['$scope', '$q', '$state', '$stateParams', '$location', 'uiTools', 'authService']; 

    function loginController($scope, $q, $state, $stateParams, $location, uiTools, authService) {


        $scope.loginForm = {
            login: null,
            password: null
        };

        $scope.validationRules = {
            login: { custom: uiTools.validate.getValidator('required') },
            password: { custom: uiTools.validate.getValidator('required') }
        };

        $scope.login = function () {
           
            function onSuccess() {
                if ($stateParams.redirect) {
                    $location.path($stateParams.redirect);
                } else {
                    $state.go('home');
                }
            }
            
            function onError(response) {
                var message = '';
                var error = JSON.parse(response.error_description);
                if (response.error === 'LoginValidationError') {
                    message = _.pluck(error.faults, 'message').join('\r\n');
                } else {
                    message = error.message;
                }
                uiTools.notify({
                    type: 'error',
                    title: 'Не удалось войти',
                    message: message
                });
                $scope.loginForm.password = null;
            }
            
            return authService
                .login($scope.loginForm)
                .then(onSuccess, onError);

        };
    }
})();