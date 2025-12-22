using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NgoUyenNguyen
{
    [AddComponentMenu( "UI/Tab Button" )]
    public class TabButton : Image, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        public enum InteractType
        {
            None,
            ChangeColor,
            ChangeSprite
        }
        
        [SerializeField, Tooltip( "The tab group this button belongs to." )]
        private TabGroup tabGroup;
        
        [SerializeField] private InteractType interactOption = InteractType.None;
        [SerializeField] private Color selectedColor = Color.white;
        [SerializeField] private Color hoveredColor = Color.white;
        private Color defaultColor;
        [SerializeField] private Sprite selectedSprite;
        [SerializeField] private Sprite hoveredSprite;
        private Sprite defaultSprite;
        
        [SerializeField] private UnityEvent onSelected;
        [SerializeField] private UnityEvent onDeselected;
        [SerializeField] private UnityEvent onHovered;
        [SerializeField] private UnityEvent onUnhovered;

        public TabGroup TabGroup
        {
            get => tabGroup;
            set
            {
                tabGroup = value;
                tabGroup.Subcribe(this);
            }
        }

        public InteractType InteractOption
        {
            get => interactOption;
            set => interactOption = value;
        }

        public Color SelectedColor
        {
            get => selectedColor;
            set => selectedColor = value;
        }

        public Color HoveredColor
        {
            get => hoveredColor;
            set => hoveredColor = value;
        }

        public Sprite SelectedSprite
        {
            get => selectedSprite;
            set => selectedSprite = value;
        }

        public Sprite HoveredSprite
        {
            get => hoveredSprite;
            set => hoveredSprite = value;
        }

        protected override void Awake()
        {
            defaultColor = color;
            defaultSprite = sprite;
            tabGroup?.Subcribe(this);
        }

        public UnityEvent OnSelected => onSelected;

        public UnityEvent OnDeselected => onDeselected;

        public UnityEvent OnHovered => onHovered;

        public UnityEvent OnUnhovered => onUnhovered;

        public void OnPointerEnter(PointerEventData eventData) => tabGroup?.OnTabEnter(this);

        public void OnPointerClick(PointerEventData eventData) => tabGroup?.OnTabSelected(this);

        public void OnPointerExit(PointerEventData eventData) => tabGroup?.OnTabExit(this);

        public void Select()
        {
            switch (interactOption)
            {
                case InteractType.ChangeColor: color = selectedColor; break;
                case InteractType.ChangeSprite: sprite = selectedSprite; break;
            }
            onSelected?.Invoke();
        }

        public void Deselect()
        {
            color = defaultColor;
            sprite = defaultSprite;
            onDeselected?.Invoke();
        }

        public void Hover(bool changeColor = true)
        {
            if (changeColor)
            {
                switch (interactOption)
                {
                    case InteractType.ChangeColor: color = hoveredColor; break;
                    case InteractType.ChangeSprite: sprite = hoveredSprite; break;
                }
            }

            onHovered?.Invoke();
        }
        
        public void Unhover(bool changeColor = true)
        {
            if (changeColor)
            {
                color = defaultColor;
                sprite = defaultSprite;
            }
            onUnhovered?.Invoke();
        }
    }
}
