﻿(function () {
    'use strict';

    angular
        .module('vabank.webapp')
        .controller('cardListController', cardListController);

    cardListController.$inject = ['$scope', '$state', 'uiTools', 'cardManagementService'];

    function cardListController($scope, $state, uiTools, cardManagementService) {
        var account = $scope.account;
        var AccountCard = cardManagementService.AccountCard;
        var Card = cardManagementService.Card;

        var loadData = function() {
            var promise = AccountCard.query({ accountNo: account.accountNo, isActive: true }).$promise;
            $scope.itemLoading.addPromise(promise);
            promise.then(function (cards) {
                $scope.cards = cards;
            });
        };

        $scope.cards = [];

        $scope.displayedCards = angular.copy($scope.cards);

        $scope.isExpired = function (card) {
            var now = moment().utc().startOf('day');
            var expires = moment.utc(card.expirationDateUtc).startOf('day');
            return expires.isSame(now) || expires.isBefore(now);
        }

        $scope.deactivate = function (card) {
            var params = {                
                cardId: card.cardId,
                assigned: false
            };
            var promise = Card.activate(params).$promise;
            $scope.itemLoading.addPromise(promise);
            promise.then(function(response) {
                $scope.cards = _.without($scope.cards, card);
                uiTools.notify({                    
                    type: 'success',
                    message: response.message
                });
            });

        };

        $scope.$on('accountTabChanged', function (e, tabName) {
            if (tabName === 'cards') {
                loadData();
            }
        });

        loadData();
    }
})();
