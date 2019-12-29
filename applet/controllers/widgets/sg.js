/// <reference path="../../.ref/js/santedb.js"/>
/*
 * Portions Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 * Portions Copyright 2019-2019 SanteSuite Contributors (See NOTICE)
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
 * User: Justin Fyfe
 * Date: 2019-9-27
 */
angular.module('santedb').controller('SanteGuardObjectAccessController', ["$scope", "$rootScope", function($scope, $rootScope) {

    $scope.auditFilter = {
            _noexec : true,
            'object.id' : EmptyGuid 
        };

    $scope.$watch("scopedObject", function(n,o) {
        if(n) {
            $scope.auditFilter = {
                'object.id' : n.id
            };
        }
    });

    // Render the outcome
    $scope.renderOutcome = function (audit) {
        switch (audit.outcome) {
            case "Success":
                return `<span class='badge badge-success'><i class='fas fa-check'></i> ${SanteDB.locale.getString("ui.model.audit.outcome.success")}</span>`;
            case "MinorFail":
                return `<span class='badge badge-warning'><i class='fas fa-info-circle'></i> ${SanteDB.locale.getString("ui.model.audit.outcome.warning")}</span>`;
            case "SeriousFail":
                return `<span class='badge badge-warning'><i class='fas fa-info-circle'></i> ${SanteDB.locale.getString("ui.model.audit.outcome.error")}</span>`;
            case "EpicFail":
                return `<span class='badge badge-danger'><i class='fas fa-exclamation-circle'></i> ${SanteDB.locale.getString("ui.model.audit.outcome.epic")}</span>`;
            default:
                return audit.outcome;
        }
    };

    // Render the timestamp
    $scope.renderTimestamp = function (audit) {
        return moment(audit.timestamp).format('YYYY-MM-DD HH:mm:ss Z');
    }

    // Render the action column
    $scope.renderAction = function (audit) {

        var retVal = "";
        switch (audit.action) {
            case "Read":
                retVal = "<i class='fas fa-database text-success fa-fw'></i> ";
                break;
            case "Create":
            case "Update":
            case "Delete":
                retVal = "<i class='fas fa-database text-danger fa-fw'></i> ";
                break;
            case "Execute":
                retVal = "<i class='fas fa-play'></i> ";
                break;
        }
        retVal += audit.action;
        return retVal;
    }

    // Render the event column
    $scope.renderEvent = function (audit) {


        var requestor = audit.actor.find((a) => a.isReq);
        var user = requestor.uname;
        var ipMachine = requestor.apId;
        var action = audit.action;
        if(audit.event == "Query" && audit.action == "Execute")
            action = "Query";
        var friendlyMessage = SanteDB.locale.getString(`ui.audit.log.${action}.friendly`);
        return $scope.$eval(friendlyMessage, { 
            user: user,
            ipMachine: ipMachine
        });
    }

    

}]);
