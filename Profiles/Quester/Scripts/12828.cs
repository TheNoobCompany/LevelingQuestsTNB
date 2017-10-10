/* Use Item With HotSpots
Check if there is HotSpots in the objective */

try
{
	if (questObjective.Hotspots.Count <= 0)
	{
		/* Objective CSharpScript with script UseItemWithHotSpots requires valid "Hotspots" */
		Logging.Write("UseItemWithHotSpots requires valid 'HotSpots'.");
		questObjective.IsObjectiveCompleted = true;
		return false;
	}
	
	List<int> meatIds = new List<int>();
	meatIds.Add(192693);
	meatIds.Add(191567);
	
	WoWGameObject node = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetWoWGameObjectById(meatIds));
	
	if (ItemsManager.GetItemCount(questObjective.UseItemId) <= 0 && !node.IsValid)
	{
		//Logging.Write("PAS ITEM");
		
		WoWGameObject dispenser = ObjectManager.GetNearestWoWGameObject(ObjectManager.GetWoWGameObjectById(questObjective.ExtraInt));
		
		Point posDispenser = new Point("6401.539 ; -1119.357 ; 425.7128 ; None");
		
		if(posDispenser.DistanceTo(ObjectManager.Me.Position) >= 5)
		{
			nManager.Wow.Helpers.MovementManager.GoToLocationFindTarget(posDispenser);
			return false;
		}
		
		MovementManager.StopMove();
		MountTask.DismountMount();
		
		if(dispenser.IsValid)
		{
			MovementManager.Face(dispenser);
			Interact.InteractWith(dispenser.GetBaseAddress); 
			nManager.Wow.Helpers.Quest.SelectGossipOption(1);
		}
		if (ItemsManager.GetItemCount(questObjective.UseItemId) > 0)
			Logging.Write("GOOD");
	}
	
	WoWUnit unit = ObjectManager.GetNearestWoWUnit(ObjectManager.GetWoWUnitByEntry(questObjective.Entry, questObjective.IsDead), questObjective.IgnoreNotSelectable, questObjective.IgnoreBlackList,
		questObjective.AllowPlayerControlled);
		
	if(node.IsValid)
		unit = new WoWUnit(0); //Reset Mams if we have meat to pickup
	
	Point pos = ObjectManager.Me.Position; /* Initialize or getting an error */
	//int q = QuestID; /* not used but otherwise getting warning QuestID not used */
	uint baseAddress = 0;

	/* If Entry found continue, otherwise continue checking around HotSpots */
	if ((unit.IsValid && (!nManagerSetting.IsBlackListedZone(unit.Position) && !nManagerSetting.IsBlackListed(unit.Guid) || questObjective.IgnoreBlackList )) ||
		(node.IsValid && (!nManagerSetting.IsBlackListedZone(node.Position) && !nManagerSetting.IsBlackListed(node.Guid) || questObjective.IgnoreBlackList)))
	{
		if(nManager.Wow.Helpers.PathFinder.FindPath(node.IsValid ? node.Position : unit.Position).Count <= 0)
		{
			nManagerSetting.AddBlackList(node.IsValid ? node.Guid : unit.Guid, 30*1000);
			return false;
		}
		
		if (questObjective.IgnoreFight && unit.GetDistance <= 20) //Dont ignore fight if we are too far from the mob... 
			nManager.Wow.Helpers.Quest.GetSetIgnoreFight = true;
		/* Entry found, GoTo */
	
		//Pre Select Target
		if (node.IsValid && node.Position.DistanceTo(ObjectManager.Me.Position) <= 60 && ObjectManager.Target.Guid != node.Guid)
		{
			Interact.InteractWith(node.GetBaseAddress);
		}
		else if (unit.IsValid && unit.Position.DistanceTo(ObjectManager.Me.Position) <= 60 && ObjectManager.Target.Guid != unit.Guid)
		{	
			Lua.LuaDoString("ClearTarget()");
			
			if(questObjective.ExtraString == "InteractWith")
			{
				Interact.InteractWith(unit.GetBaseAddress);
			}
			else
			{
				ObjectManager.Me.Target = unit.Guid;
			}
		}
		
		if (node.IsValid)
		{
			unit = new WoWUnit(0);
			baseAddress = MovementManager.FindTarget(node, questObjective.Range);
		}
		if (unit.IsValid)
		{
			node = new WoWGameObject(0);
			baseAddress = MovementManager.FindTarget(unit, questObjective.Range);
		}	
	
		if((node.IsValid && node.GetDistance < questObjective.Range) || (unit.IsValid && unit.GetDistance <= questObjective.Range))
		{
			//Logging.Write("TARGET REACHED" + unit.GetDistance);
			/* Target Reached */
			MovementManager.StopMove();
			MountTask.DismountMount();
		}
		else
		{
			if (MovementManager.InMovement)
				return false;
			if (questObjective.IgnoreNotSelectable)
			{
				if ((node.IsValid && node.GetDistance > questObjective.Range) || (unit.IsValid && unit.GetDistance > questObjective.Range))
					return false;
			}
			else
			{
				if (baseAddress <= 0)
					return false;
				if (baseAddress > 0 && ((node.IsValid && node.GetDistance > questObjective.Range) || (unit.IsValid && unit.GetDistance > questObjective.Range)))
					return false;
				
			}
		}
		
		/* Target Reached */
		MovementManager.StopMove();
		MountTask.DismountMount();
		
		Thread.Sleep(500);
		
		if (node.IsValid)
		{
			MovementManager.Face(node);
			MovementManager.Face(node);
			Interact.InteractWith(node.GetBaseAddress); 
		}
		else if (unit.IsValid)
		{
			MovementManager.Face(unit);
		}
		
		Thread.Sleep(100 + Usefuls.Latency); /* ZZZzzzZZZzz */

		
		if(unit.IsValid) //Only use item on mams
		{
			if (ItemsManager.GetItemCount(questObjective.UseItemId) <= 0 || ItemsManager.IsItemOnCooldown(questObjective.UseItemId) || !ItemsManager.IsItemUsable(questObjective.UseItemId))
				return false;
			
			
			ItemsManager.UseItem(ItemsManager.GetItemNameById(questObjective.UseItemId));

			Thread.Sleep(Usefuls.Latency + 250);

			/* Wait for the Use Item cast to be finished, if any */
			while (ObjectManager.Me.IsCast)
			{
				Thread.Sleep(Usefuls.Latency);
			}
		}
		
		if (node.IsValid)
		{
			nManagerSetting.AddBlackList(node.Guid, 60*1000);
		}
		else if (unit.IsValid)
		{
			
			Interact.InteractWith(unit.GetBaseAddress); //Interact With Unit to Attack it
			nManagerSetting.AddBlackList(unit.Guid, 60*1000);
		}

		/* Wait if necessary */
		if (questObjective.WaitMs > 0)
			Thread.Sleep(questObjective.WaitMs);

		nManager.Wow.Helpers.Quest.GetSetIgnoreFight = false;
		return true;
	}
		/* Move to Zone/Hotspot */
	else if (!MovementManager.InMovement)
	{
		nManager.Wow.Helpers.Quest.GetSetIgnoreFight = false;
		if (questObjective.PathHotspots[nManager.Helpful.Math.NearestPointOfListPoints(questObjective.PathHotspots, ObjectManager.Me.Position)].DistanceTo(ObjectManager.Me.Position) > 5)
		{
			if(nManager.Wow.Helpers.Quest.TravelToQuestZone(questObjective.PathHotspots[nManager.Helpful.Math.NearestPointOfListPoints(questObjective.PathHotspots, ObjectManager.Me.Position)],
				ref questObjective.TravelToQuestZone, questObjective.ContinentId, questObjective.ForceTravelToQuestZone))
				return false;
			MovementManager.Go(PathFinder.FindPath(questObjective.PathHotspots[nManager.Helpful.Math.NearestPointOfListPoints(questObjective.PathHotspots, ObjectManager.Me.Position)]));
		}
		else
		{
			MovementManager.GoLoop(questObjective.PathHotspots);
		}
	}
}
catch (Exception ex)
{
	Logging.Write(ex.Message);
}
finally
{
	/*nManager.Wow.Helpers.Quest.GetSetIgnoreFight = false;*/
}