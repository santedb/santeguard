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
angular.module('santedb').controller('SanteGuardEmrObjectAccessController', ["$scope", "$rootScope", function($scope, $rootScope) {

  
    $scope.history = [];


    /**
     * @summary Fetches the edit history of the object
     * @param {*} objectId The identifier of the object to fetch history for
     */
    async function fetchHistory(objectId) {
        try {
            var er = await SanteDB.resources.entityRelationship.findAsync({ 
                "relationshipType": "97730a52-7e30-4dcd-94cd-fd532d111578", // MDM
                "target" : objectId // Where this is the target
            }, "reverseRelationship");

            var auditBundle = null;
            if(er.resource) // there was an MDM reroute
                auditBundle = await SanteDB.resources.audit.findAsync({
                    "object.id" : [ er.resource[0].holder, er.resource[0].target ],
                    "action" : [ "!Execute", "!Read" ]
                });
            else 
                auditBundle = await SanteDB.resources.audit.findAsync({
                    "object.id" : objectId,
                    "action" : [ "!Execute", "!Read" ]
                });

            $scope.history = auditBundle.resource;
            $scope.$apply();
        }
        catch(e) {
            console.warn(e);
        }
    }

    $scope.$watch("scopedObject", function(n,o) {
        if(n) {
            
            // Set the contents of the view
            fetchHistory(n.id);
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
        if(!requestor)
            requestor = {};
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
