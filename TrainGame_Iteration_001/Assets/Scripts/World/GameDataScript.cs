using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavedPlayerData
{
	//public int carriageCount;
	public SavedTurretUpgradeData blastTower;
	public SavedTurretUpgradeData missileTower;
	public SavedTurretUpgradeData arcTower;
    public SavedTurretUpgradeData beamTower;
}

public struct SavedTurretUpgradeData
{
	public bool purchased;                  // Indicates whether we are currently capable of building this type of turret
	public int maxHPLevel;                  //
	public int maxArmorLevel;               //
	public int damageLevel;                 //
    public int currentLevel;
    public bool upgradeEnabled_1;           // Unique upgrade 1 has been purchased or not?
	public bool upgradeEnabled_2;           // Unique upgrade 2 has been purchased or not? Specific properties of these nique upgrades can be defined within  each of the turret class themselves
    public bool upgradeEnabled_3;           // Unique upgrade 1 has been purchased or not?

    public SavedTurretUpgradeData(bool enabled, int HPlevel, int armorLevel, int attackLevel, int towerLevel, bool upgrade1, bool upgrade2, bool upgrade3)
	{
		purchased = enabled;
		maxHPLevel = HPlevel;
		maxArmorLevel = armorLevel;
		damageLevel = attackLevel;
        currentLevel = towerLevel;
		upgradeEnabled_1 = upgrade1;
		upgradeEnabled_2 = upgrade2;
        upgradeEnabled_3 = upgrade3;
    }
}

public struct TrainUpgradeData
{
	public int[] maxHP;                     // Contain maximum HP at each upgrade level
	public int[] maxArmor;                  // Contain maximum Armor at each upgrade level
}

public struct TurretUpgradeData
{
	public int[] maxHP;                     // Contain maximum HP at each upgrade level
	public int[] maxArmor;                  // Contain maximum Armor at each upgrade level
	public int[] damage;                    // contain attack power at each upgrade level
    public int[] towerlvl;

    public TurretUpgradeData(int[] health, int[] armor, int[] power, int[] level)
	{
		maxHP = health;
		maxArmor = armor;
		damage = power;
        towerlvl = level;
	}
}
public struct TurretUpgradeCollectionData
{
	public TurretUpgradeData BlastTower;    //
	public TurretUpgradeData MissileTower;  //
	public TurretUpgradeData ArcTower;      //
    public TurretUpgradeData BeamTower;      //
}

public class GameDataScript : MonoBehaviour
{

	private TurretUpgradeCollectionData _turretCollectionData;
	//private TrainUpgradeData _trainUpgradeData;
	private SavedPlayerData _savedPlayerData = new SavedPlayerData();

	// Use this for initialization
	void Start()
	{
		PopulateStaticGameData();
		PopulatePlayerData();        
    }

	// Update is called once per frame
	void Update()
	{
       
    }

	/// <summary>
	/// Populate the fields containing data on Turret and carriages stats upgrades
	/// </summary>
	public void PopulateStaticGameData()
	{
		TurretUpgradeData blastTowerUpgrades = new TurretUpgradeData(new int[] { 100, 200, 300, 400 },          // max HP at each level
																						new int[] { 100, 200, 300, 400 },           // Armor
																						new int[] { 100, 200, 300, 400 },           // Damage  
                                                                                        new int[] {   1,   2,   3,   4});          
		TurretUpgradeData missileTowerUpgrades = new TurretUpgradeData(new int[] { 100, 200, 300, 400 },        // HP
																						new int[] { 100, 200, 300, 400 },           // Armor
                                                                                        new int[] { 100, 200, 300, 400 },           // Damage  
                                                                                        new int[] {   1,   2,   3,   4 });          // Level
        TurretUpgradeData arcTowerUpgrades = new TurretUpgradeData(new int[] { 100, 200, 300, 400 },            // HP
																						new int[] { 100, 200, 300, 400 },           // Armor
                                                                                        new int[] { 100, 200, 300, 400 },           // Damage  
                                                                                        new int[] {   1,   2,   3,   4 });          // Level
        TurretUpgradeData beamTowerUpgrades = new TurretUpgradeData(new int[] { 100, 200, 300, 400 },            // HP
                                                                                new int[] { 100, 200, 300, 400 },           // Armor
                                                                                new int[] { 100, 200, 300, 400 },           // Damage  
                                                                                new int[] { 1, 2, 3, 4 });          // Level
                                                                                                                    //Add more here

        //
        _turretCollectionData.BlastTower = blastTowerUpgrades;
		_turretCollectionData.MissileTower = missileTowerUpgrades;
		_turretCollectionData.ArcTower = arcTowerUpgrades;
        _turretCollectionData.BeamTower = beamTowerUpgrades;
	}

	/// <summary>
	/// Get the data on the player's current progress to update 
	/// </summary>
	public void PopulatePlayerData()
	{
		// These will be replaced once we finish the save system
		//_savedPlayerData.carriageCount = 3;
		SavedTurretUpgradeData blastTower = new SavedTurretUpgradeData(true, 100, 100, 100, 1, false, false, false);
		SavedTurretUpgradeData missileTower = new SavedTurretUpgradeData(false, 100, 100, 100, 1, false, false, false);
		SavedTurretUpgradeData arcTower = new SavedTurretUpgradeData(false, 100, 100, 100, 1, false, false, false);
        SavedTurretUpgradeData beamTower = new SavedTurretUpgradeData(false, 100, 100, 100, 1, false, false, false);
        _savedPlayerData.blastTower = blastTower;
		_savedPlayerData.arcTower = arcTower;
		_savedPlayerData.missileTower = missileTower;
        _savedPlayerData.beamTower = beamTower;
    }

	public SavedPlayerData GetSavedPlayerData
	{
       get { return _savedPlayerData; }
       set { _savedPlayerData = value; }
        
	}

	public TurretUpgradeCollectionData GetTurretUpradeCollection()
	{
		return _turretCollectionData;
	}

	//public TrainUpgradeData GetTrainUpgradeData()
	//{
	//	return _trainUpgradeData;
	//}
}
