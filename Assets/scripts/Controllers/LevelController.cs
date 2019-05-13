using System.Collections.Generic;
using UnityEngine;

namespace Controllers
{

    /**
     * spawn/delete enemies, warriors and other characters (elements) in the level.
     * keep track of the above characters in the level.
     * manages the environmental changes in the level.
     */
    public class LevelController : MonoBehaviour
    {
        //boss prefabs
        public GameObject bossPrefab;
        //mobs prefabs
        public GameObject firstWaveMobPrefab;
        public GameObject secondWaveMobPrefab;
        public GameObject thirdWaveMobAttackerPrefab;
        public GameObject thirdWaveMobHealerPrefab;
        //portals prefabs
        public GameObject portalAPrefab;
        public GameObject portalBPrefab;
        //warriors prefabs
        public GameObject roguePrefab;
        public GameObject magePrefab;
        
        private GameObject _boss;
        private GameObject _rogue;
        private GameObject _mage;
        
        public static LevelController instance = null;
        
        //a list of warriors, it needs to be passed to the enemies' rule engine, for them to make the decisions (ie, who to attack)
        List<GameObject> _warriors = new List<GameObject>();
        //a list of the current living enemies in the level. these needs to be passed to warriors' rule engine.
        List<GameObject> _enemies = new List<GameObject>();
        
        //Awake is always called before any Start functions
        void Awake()
        {
            //Check if instance already exists
            if (instance == null)
            {
                //if not, set instance to this
                instance = this;
            }
            //If instance already exists and it's not this:
            else if (instance != this)
            {
                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                Destroy(gameObject);    
            }
            //Sets this to not be destroyed when reloading scene
            DontDestroyOnLoad(gameObject);
           
            //init the first batch of enemy (in the beginning just the boss)
            _boss = Instantiate(bossPrefab);
            //the boss will always be the first element of our enemies list.
            _enemies.Add(_boss);

            _rogue = Instantiate(roguePrefab);
            _mage = Instantiate(magePrefab);
            
            _warriors.Add(_rogue);
            _warriors.Add(_mage);
            
            //todo: init the in battle game ui.
        }

        public void startFirstWave()
        {
            Debug.Log("start the first wave of mobs....");
        }

        public void startSecondWave()
        {
            Debug.Log("start the second wave of mobs....");            
        }
        
        
        
        
    } 
    
}
