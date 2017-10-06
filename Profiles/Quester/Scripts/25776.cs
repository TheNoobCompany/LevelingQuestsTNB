/*
This script is using a thread to make the method StartFight Async since we want to be
able to check the health of the mob being attacked (to do an action)
*/

System.Threading.Thread _worker2;

/* Move to Zone/Hotspot */


nManager.Wow.ObjectManager.WoWUnit unit = nManager.Wow.ObjectManager.ObjectManager.GetNearestWoWUnit(nManager.Wow.ObjectManager.ObjectManager.GetWoWUnitByEntry(questObjective.Entry));

if(unit.IsValid)
{
	nManager.Wow.Helpers.Quest.GetSetIgnoreFight = true;

	while(ObjectManager.Me.Position.DistanceTo(unit.Position) >= 5)
	{
		if (ObjectManager.Me.IsDeadMe || (ObjectManager.Me.InCombat && !ObjectManager.Me.IsMounted))
		{
			return false;
		}
		MovementManager.FindTarget(unit, 5);
		Thread.Sleep(500);
	}
	
	_worker2 = new System.Threading.Thread(() => nManager.Wow.Helpers.Fight.StartFight(unit.Guid));
	Logging.Write("ICI2");
	Thread.Sleep(500);

	_worker2.Start();

	nManager.Wow.ObjectManager.WoWUnit dragonForm = nManager.Wow.ObjectManager.ObjectManager.GetNearestWoWUnit(nManager.Wow.ObjectManager.ObjectManager.GetWoWUnitByEntry(questObjective.ExtraInt));

	
	Thread.Sleep(500);

	while (!dragonForm.IsValid && ObjectManager.Me.InCombat)
	{
		if(ObjectManager.Me.IsDeadMe)
		{
			return false;
		}
		Thread.Sleep(50);

	}
	
	nManager.Wow.Helpers.Fight.InFight= false;
	nManager.Wow.Helpers.Fight.StopFight();
	
	Logging.Write("Dragon Form - Using Item");
	Thread.Sleep(2500);
	
	ItemsManager.UseItem(ItemsManager.GetItemNameById(questObjective.UseItemId));

	nManager.Wow.Helpers.Quest.GetSetIgnoreFight = false;
	
	_worker2 = null;
	Thread.Sleep(200);
	return false;
}else if (!MovementManager.InMovement)
{
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