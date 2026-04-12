using System;
using UnityEngine;

namespace TacticFantasy.Adapters
{
    public class InputHandler : MonoBehaviour
    {
        public event Action<int, int> OnTileClicked;
        public event Action<int, int> OnUnitClicked;

        private Camera _mainCamera;
        private const float TILE_SIZE = 1f;

        public void Start()
        {
            _mainCamera = Camera.main;
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleMouseClick();
            }
        }

        private void HandleMouseClick()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 hitPoint = hit.point;
                int x = Mathf.RoundToInt(hitPoint.x / TILE_SIZE);
                int y = Mathf.RoundToInt(hitPoint.z / TILE_SIZE);

                OnTileClicked?.Invoke(x, y);
                OnUnitClicked?.Invoke(x, y);
            }
        }
    }
}
