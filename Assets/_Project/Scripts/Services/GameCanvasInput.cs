using Superglazka.Games.Peripheral;
using Superglazka.Games.Runner;
using Superglazka.Games.Blink;
using Superglazka.Games.Gym;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Superglazka.Services
{
    public class GameCanvasInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField] private PeripheralGame _peripheralGame;
        [SerializeField] private RunnerGame _runnerGame;
        [SerializeField] private BlinkGame _blinkGame;
        [SerializeField] private Games.Gym.GymGame _gymGame;

        public void OnPointerDown(PointerEventData eventData)
        {
            _blinkGame?.OnPointerDown();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _blinkGame?.OnPointerUp();
            _gymGame?.OnActionReleased();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_peripheralGame != null && _peripheralGame.enabled)
                _peripheralGame.OnTap(eventData.position);
            if (_runnerGame != null && _runnerGame.enabled)
                _runnerGame.Jump();
        }
    }
}
