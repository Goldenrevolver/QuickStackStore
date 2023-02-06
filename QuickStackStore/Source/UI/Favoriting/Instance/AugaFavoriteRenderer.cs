using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace QuickStackStore.UI.Favoriting
{
    internal class AugaFavoriteRenderer: FavoriteRendererInstance
    {
        public AugaFavoriteRenderer()
            : base(null)
        {
            //no-op
        }

        public override Image GetOrApplyIndicatorImage(List<InventoryGrid.Element> elements, int index)
        {
            // Use the triangle indicator in Auga for favoriting
            // If we haven't found and set the indicator image yet do so now.
            if (indicator is null)
            {
                indicator = elements[index].m_food.sprite;
            }

            // If we already have the favoriting indicator for this element, just return it.
            Image obj = elements[index].m_go.transform.Find("favorite")?.GetComponent<Image>(); ;
            if (obj != null)
            {
                return obj;
            }

            // Otherwise create it and return it.
            return CreateIndicatorImage(elements[index].m_go.transform, elements[index]);
        }

        private Image CreateIndicatorImage(Transform parent, InventoryGrid.Element baseElement)
        {
            // Duplicate the icon to be used as the indicator image.
            Image obj = Object.Instantiate(baseElement.m_icon, parent);

            // Rotate and position the favoriting indicator.
            obj.rectTransform.Rotate(Vector3.forward, 90f);
            obj.rectTransform.anchoredPosition = new Vector2(28, -28);
            obj.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 7);
            obj.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 7);

            // Set the name for easy finding later.
            obj.name = "favorite";

            // Set the image to the indicator image.
            obj.sprite = indicator;

            return obj;
        }
    }
}
