using UnityEngine;

namespace CharacterControllers {
    //animation is in fbx body, so animation event callback is assumed (by unity) to be a function inside a MonoBehavior
    //script, attached to the body. This script is attached to the body, so that unity can find the animation callback.
    //it delegate the real work to playerCtrl attached to the parent of the body.
    public class PlayerAnimationCallback : MonoBehaviour {

        private PlayerCtrl playerCtrl;
        private void Awake() {
            playerCtrl = GetComponentInParent<PlayerCtrl>();
        }

        //animation callback, used in animation event, of rogue 51 attack animation
        public void onAttackFinish() {
            playerCtrl.onAttackFinish();
        }
    }
}