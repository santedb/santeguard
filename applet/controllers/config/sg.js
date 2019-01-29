angular.module('santedb').controller('SanteGuardController', ['$scope', '$rootScope', function ($scope, $rootScope) {
    
    // Watch the configuration
    $scope.$parent.$watch("config", function(n, o) {
        if(n) {
            $scope.config = n;
            $scope.sgConfig = $scope.config.others.find(function(s) {
                return s.$type == "SanteGuard.Configuration.SanteGuardConfiguration, SanteGuard.Core";
            });

        }
    });
    
    
}]);