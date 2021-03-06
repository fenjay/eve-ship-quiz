﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eve_api
{
    internal interface IEveDataAccess
    {
        //-----------------------Versus 
        int GetEveIDForCharacterFromLocal(string characterName);
        string GetKillboardResultsFromLocal(int characterId);
        bool SaveKillboardResultsToLocal(string kbJson);
        bool SaveEveCharacterIDToLocal(int characterId, string characterName);
        int? GetMaxKillIdFromLocal(int characterId);
        List<string> GetLocalKillResults(int characterId);
        void InsertKillJson(int characterId, int killID, string killJson);



        List<string> GetRandomFromShipTypeView(int nbrShips, string column, List<int> quizLevel, List<string> excludeNames, List<int> excludeIds, List<string> excludeTypes);
        ShipFullInfo GetShipFullInfo(string shipName);
        string GetShipNameCSV(string shipNamesCSV);



    }
}
