using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuickStackStore.UI.Favoriting
{
    internal class DefaultFavoriteRenderer : FavoriteRendererInstance
    {

        public DefaultFavoriteRenderer(Sprite indicator)
:            base(indicator)
        {
            // no-op
        }

        public override Image GetOrApplyIndicatorImage(List<InventoryGrid.Element> elements, int index)
        {
            // If we already have the favoriting indicator for this element, just return it.
            if (elements[index].m_queued.transform.childCount > 0)
            {
                return elements[index].m_queued.transform.GetChild(0).GetComponent<Image>();
            }

            // Otherwise create it and return it.
            return CreateIndicatorImage(elements[index].m_queued);
        }

        private Image CreateIndicatorImage(Image baseImg)
        {
            // set m_queued parent as parent first, so the position is correct
            var obj = Object.Instantiate(baseImg, baseImg.transform.parent);
            // change the parent to the m_queued image so we can access the new image without a loop
            obj.transform.SetParent(baseImg.transform);
            // set the new indicator image
            obj.sprite = indicator;

            return obj;
        }
    }
}
