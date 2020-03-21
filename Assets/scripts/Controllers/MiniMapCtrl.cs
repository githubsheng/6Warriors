using UnityEngine;

namespace Controllers {
    public class MiniMapCtrl : MonoBehaviour {
        public Transform topRightRefPos;
        public Transform bottomLeftRefPos;
        public Transform topRightRefPos2;
        public Transform bottomLeftRefPos2;
        public Transform player;
        private float scale;
        public GameObject playerIcon;

        private void Start() {
            scale = (topRightRefPos.position.z - bottomLeftRefPos.position.z) /
                    (topRightRefPos2.position.z - bottomLeftRefPos2.position.z);
        }

        private void Update() {
            Vector3 playerPosition = player.position;
            Vector3 blPosition = bottomLeftRefPos.position;
            Vector3 blPosition2 = bottomLeftRefPos2.position;
            float x = (playerPosition.x - blPosition.x) / scale + blPosition2.x;
            float z = (playerPosition.z - blPosition.z) / scale + blPosition2.z;
            gameObject.transform.position = new Vector3(x, 10, z);
            playerIcon.transform.rotation = player.rotation;
        }
    }
}