using UnityEngine;
using UnityEngine.EventSystems;

namespace Superglazka.Services
{
    public class GameInput : MonoBehaviour
    {
        public static GameInput Instance { get; private set; }

        public bool IsTouch => Input.touchSupported && Application.isMobilePlatform;
        public Vector2 TouchPosition { get; private set; }
        public bool TouchBegan { get; private set; }
        public bool TouchEnded { get; private set; }
        public bool TouchHeld { get; private set; }
        public Vector2 TouchDelta { get; private set; }

        private Vector2 _previousTouchPos;
        private int _touchId = -1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            TouchBegan = false;
            TouchEnded = false;
            TouchDelta = Vector2.zero;

            if (IsTouch && Input.touchCount > 0)
            {
                HandleTouchInput();
            }
            else
            {
                HandleMouseInput();
            }
        }

        private void HandleTouchInput()
        {
            if (_touchId == -1)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began && !IsPointerOverUI(touch.position))
                    {
                        _touchId = touch.fingerId;
                        TouchPosition = touch.position;
                        _previousTouchPos = touch.position;
                        TouchBegan = true;
                        TouchHeld = true;
                        break;
                    }
                }
            }
            else
            {
                bool found = false;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var touch = Input.GetTouch(i);
                    if (touch.fingerId == _touchId)
                    {
                        found = true;
                        TouchPosition = touch.position;
                        TouchDelta = touch.position - _previousTouchPos;
                        _previousTouchPos = touch.position;

                        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        {
                            TouchEnded = true;
                            TouchHeld = false;
                            _touchId = -1;
                        }
                        break;
                    }
                }
                if (!found)
                {
                    TouchEnded = true;
                    TouchHeld = false;
                    _touchId = -1;
                }
            }
        }

        private void HandleMouseInput()
        {
            TouchPosition = Input.mousePosition;
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI(Input.mousePosition))
            {
                TouchBegan = true;
                TouchHeld = true;
                _previousTouchPos = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                TouchEnded = true;
                TouchHeld = false;
            }
            else if (Input.GetMouseButton(0))
            {
                TouchDelta = (Vector2)Input.mousePosition - _previousTouchPos;
                _previousTouchPos = Input.mousePosition;
            }
        }

        private bool IsPointerOverUI(Vector2 screenPos)
        {
            if (EventSystem.current == null) return false;
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = screenPos;
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 0;
        }

        public bool GetTap()
        {
            return TouchBegan;
        }
    }
}
