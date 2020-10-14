using BadAssEngi.Animations;
using RoR2;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BadAssEngi.AssetsScripts
{
    public class UIElementMover : MonoBehaviour, IDragHandler
    {
        public UIElementDocker docker;

        private Vector2 offset;

        void Start()
        {
            if (docker == null)
                docker = GetComponent<UIElementDocker>();

            var rectTransform = transform as RectTransform;

            if (Configuration.EmoteWindowPosition.Value != Vector3.one)
                rectTransform.position = Configuration.EmoteWindowPosition.Value;

            if (Configuration.EmoteWindowSize.Value != Vector3.one)
                rectTransform.sizeDelta = Configuration.EmoteWindowSize.Value;

            if (!RoR2Application.instance.mainCanvas)
                return;

            // Check if UI is OOB
            var canvasRect = RoR2Application.instance.mainCanvas.transform as RectTransform;
            var canvasBounds = new Bounds(new Vector3(canvasRect.position.x, canvasRect.position.y, 0), new Vector3(canvasRect.sizeDelta.x, canvasRect.sizeDelta.y, 1));
            var buttonBounds = new Bounds(new Vector3(rectTransform.position.x, rectTransform.position.y, 0), new Vector3(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y, 1));

            if (!buttonBounds.Intersects(canvasBounds) && EngiEmoteController.EmoteButton)
            { 
                rectTransform.position = EngiEmoteController.EmoteButton.transform.position;
                rectTransform.localPosition = EngiEmoteController.EmoteButton.transform.localPosition;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (docker != null && docker.Docked)
            {
                return;
            }

            transform.position += (Vector3)eventData.delta;

            Configuration.EmoteWindowPosition.Value = transform.position;
            Configuration.Save();
        }
    }
}
