using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSpriteReplacer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
  [SerializeField] Sprite _replacementSprite;
  Sprite _originalSprite;

  Image _spriteRenderer;

  void Awake()
  {
    _spriteRenderer = GetComponent<Image>();
    _originalSprite = _spriteRenderer.sprite;
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    _spriteRenderer.sprite = _replacementSprite;
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    _spriteRenderer.sprite = _originalSprite;
  }
}
