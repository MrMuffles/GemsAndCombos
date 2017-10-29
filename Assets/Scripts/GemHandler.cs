using UnityEngine;
using System.Collections;

namespace TBT.GemsAndCombos {

    public class GemHandler : MonoBehaviour {

        public bool mouseGem = false;
        private Vector2 dropTarget = Vector2.zero;



        private void Update () {
            if (!mouseGem) return;

            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0,0,5f);

            if (Input.GetMouseButtonUp(0))
                mouseGem = false;
        }



        public void InitDrop (int dropDistance) {
            dropTarget = new Vector2(transform.position.x, transform.position.y - dropDistance);

            StartCoroutine(DropGem());
        }

        private IEnumerator DropGem () {
            WaitForSeconds frameTime = new WaitForSeconds(0.01f);
            Vector2 startPos = transform.position;
            float lerpPercent = 0;

            while (lerpPercent <= 1) {
                transform.position = Vector2.Lerp(startPos, dropTarget, lerpPercent);
                lerpPercent += 0.05f;
                yield return frameTime;
            }

            transform.position = dropTarget;
        }
    }
}