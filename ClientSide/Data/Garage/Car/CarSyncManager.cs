using System.Collections;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Car;

public static class CarSyncManager
{
	public static IEnumerator ChangePosition(int carLoaderID, int placeNo)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		if (ClientData.Instance.loadedCars.TryGetValue(carLoaderID, out var car))
		{
			if (placeNo != car.carPosition)
			{
				MelonLogger.Msg($"Change {car.carID} position to {placeNo}.");
				car.carPosition = placeNo;
				CarSyncHooks.listenToChangePosition = false;
				GameData.Instance.carLoaders[carLoaderID].ChangePosition(placeNo);
			}
		}
	}

	public static IEnumerator DeleteCar(int carLoaderID)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		// Remove from loaded cars first
		if (ClientData.Instance.loadedCars.ContainsKey(carLoaderID))
			ClientData.Instance.loadedCars.Remove(carLoaderID);
		
		// Disable listening to prevent recursive calls
		CarSpawnHooks.listenToDelete = false;
		
		// Delete the car with proper error handling
		try
		{
			if (GameData.Instance.carLoaders != null && carLoaderID >= 0 && carLoaderID < GameData.Instance.carLoaders.Length)
			{
				GameData.Instance.carLoaders[carLoaderID].DeleteCar();
			}
		}
		catch (System.Exception ex)
		{
			MelonLogger.Error($"[CarSyncManager->DeleteCar] Error deleting car {carLoaderID}: {ex.Message}");
		}
		
		// Reset the listen flag after a delay
		MelonCoroutines.Start(ResetListenToDelete());
	}
	
	private static IEnumerator ResetListenToDelete()
	{
		yield return new WaitForSeconds(0.2f);
		CarSpawnHooks.listenToDelete = true;
	}
}