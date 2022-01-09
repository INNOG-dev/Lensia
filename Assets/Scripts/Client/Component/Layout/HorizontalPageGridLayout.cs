using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HorizontalPageGridLayout : LayoutGroup
{

    /// <summary>
    /// Which corner is the starting corner for the grid.
    /// </summary>
    public enum Corner
    {
        /// <summary>
        /// Upper Left corner.
        /// </summary>
        UpperLeft = 0,
        /// <summary>
        /// Upper Right corner.
        /// </summary>
        UpperRight = 1,
        /// <summary>
        /// Lower Left corner.
        /// </summary>
        LowerLeft = 2,
        /// <summary>
        /// Lower Right corner.
        /// </summary>
        LowerRight = 3
    }

    /// <summary>
    /// Constraint type on either the number of columns or rows.
    /// </summary>
    public enum Constraint
    {
        /// <summary>
        /// Don't constrain the number of rows or columns.
        /// </summary>
        Flexible = 0,
        /// <summary>
        /// Constrain the number of columns to a specified number.
        /// </summary>
        FixedColumnCount = 1,
        /// <summary>
        /// Constraint the number of rows to a specified number.
        /// </summary>
        FixedRowCount = 2
    }

    [SerializeField] protected Corner m_StartCorner = Corner.UpperLeft;

    /// <summary>
    /// Which corner should the first cell be placed in?
    /// </summary>
    public Corner startCorner { get { return m_StartCorner; } set { SetProperty(ref m_StartCorner, value); } }


    [SerializeField] protected Vector2 m_CellSize = new Vector2(100, 100);

    /// <summary>
    /// The size to use for each cell in the grid.
    /// </summary>
    public Vector2 cellSize { get { return m_CellSize; } set { SetProperty(ref m_CellSize, value); } }

    [SerializeField] protected Vector2 m_Spacing = Vector2.zero;

    /// <summary>
    /// The spacing to use between layout elements in the grid on both axises.
    /// </summary>
    public Vector2 spacing { get { return m_Spacing; } set { SetProperty(ref m_Spacing, value); } }

    [SerializeField] protected Constraint m_Constraint = Constraint.Flexible;

    /// <summary>
    /// Which constraint to use for the GridLayoutGroup.
    /// </summary>
    /// <remarks>
    /// Specifying a constraint can make the GridLayoutGroup work better in conjunction with a [[ContentSizeFitter]] component. When GridLayoutGroup is used on a RectTransform with a manually specified size, there's no need to specify a constraint.
    /// </remarks>
    public Constraint constraint { get { return m_Constraint; } set { SetProperty(ref m_Constraint, value); } }

    [SerializeField] protected int m_ConstraintCount = 2;

    /// <summary>
    /// How many cells there should be along the constrained axis.
    /// </summary>
    public int constraintCount { get { return m_ConstraintCount; } set { SetProperty(ref m_ConstraintCount, Mathf.Max(1, value)); } }

    public Vector2 defaultContentSize = new Vector2(315.66f, 193.61f);
    
    [SerializeField]
    private int m_PageCount = 1;

    public int pageCount { get { return m_PageCount; } set { SetProperty(ref m_PageCount, Mathf.Max(1, value)); } }


    public override void CalculateLayoutInputHorizontal()
    {

        if(m_PageCount < 1)
        {
            m_PageCount = 1;
        }

        RectTransform rectTransform = (RectTransform)transform;
        rectTransform.sizeDelta = new Vector2(defaultContentSize.x * pageCount, defaultContentSize.y);

        base.CalculateLayoutInputHorizontal();



    }

  
    public override void CalculateLayoutInputVertical()
    {

 

    }

    public override void SetLayoutHorizontal()
    {
        SetCellsAlongAxis(0);
    }

    public override void SetLayoutVertical()
    {
        SetCellsAlongAxis(1);
    }

    protected virtual void SetCellsAlongAxis(int axis)
    {
        // Normally a Layout Controller should only set horizontal values when invoked for the horizontal axis
        // and only vertical values when invoked for the vertical axis.
        // However, in this case we set both the horizontal and vertical position when invoked for the vertical axis.
        // Since we only set the horizontal position and not the size, it shouldn't affect children's layout,
        // and thus shouldn't break the rule that all horizontal layout must be calculated before all vertical layout.

        var rectChildrenCount = rectChildren.Count; //1

        if (axis == 0)
        {
            // Only set the sizes when invoked for horizontal axis, not the positions. preconfig object

            for (int i = 0; i < rectChildrenCount; i++)
            {
                RectTransform rect = rectChildren[i];


                m_Tracker.Add(this, rect,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.AnchoredPosition |
                    DrivenTransformProperties.SizeDelta);

                rect.anchorMin = Vector2.up;
                rect.anchorMax = Vector2.up;
                rect.sizeDelta = cellSize;
            }
            return;
        }

        float width = defaultContentSize.x;
        float height = defaultContentSize.y;

        int cellCountX = 1;
        int cellCountY = 1;
        if (m_Constraint == Constraint.FixedColumnCount)
        {
            cellCountX = m_ConstraintCount; //nombre d'objet par collonne 9

            if (rectChildrenCount > cellCountX) //si le nombre d'enfant est plus grand que le nombre d'element fixé par collonne 
               cellCountY = rectChildrenCount / cellCountX + (rectChildrenCount % cellCountX > 0 ? 1 : 0); // 
               //9 / 2 + (1)
               //8 / 2 + (0)
               //1 / 2 + 0
               //0 

            //9%2 = 1
            //8%2 ==
            //7%2 == 0
  
        }
        else if (m_Constraint == Constraint.FixedRowCount)
        {
            cellCountY = m_ConstraintCount;

            if (rectChildrenCount > cellCountY)
                cellCountX = rectChildrenCount / cellCountY + (rectChildrenCount % cellCountY > 0 ? 1 : 0);
        }
        else
        {
            if (cellSize.x + spacing.x <= 0)
            {
     
                cellCountX = int.MaxValue;
            }             
            else
            {
                cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));
            }
                

            if (cellSize.y + spacing.y <= 0)
            {
                cellCountY = int.MaxValue;
            }
            else
            {
                cellCountY = Mathf.Max(1, Mathf.FloorToInt((height - padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y)));
            }  // calculate numbers of elements can add in X and Y according to with and height and others paramaters
        }

        int cornerX = (int)startCorner % 2;
        int cornerY = (int)startCorner / 2;

        int cellsPerMainAxis, actualCellCountX, actualCellCountY;

        cellsPerMainAxis = cellCountX;

        actualCellCountX = Mathf.Clamp(cellCountX, 1, rectChildrenCount);
        actualCellCountY = Mathf.Clamp(cellCountY, 1, Mathf.CeilToInt(rectChildrenCount / (float)cellsPerMainAxis));
        


        Vector2 requiredSpace = new Vector2(
            actualCellCountX * cellSize.x + (actualCellCountX - 1) * spacing.x,
            actualCellCountY * cellSize.y + (actualCellCountY - 1) * spacing.y
        );

        Vector2 startOffset = new Vector2(
            GetStartOffset(0, requiredSpace.x),
            GetStartOffset(1, requiredSpace.y)
        );

        for (int i = 0; i < rectChildrenCount; i++)
        {
            LayoutPageElement layout = rectChildren[i].GetComponent<LayoutPageElement>();

            int page = 1;

            if (layout != null)
            {
                page = layout.elementPage;

                Debug.Log(page);
            }

            if (page > pageCount) page = pageCount;


            int positionX = page * i % cellsPerMainAxis;
            int positionY = page * i / cellsPerMainAxis;

            if (cornerX == 1)
                positionX = actualCellCountX - 1 - positionX;
            if (cornerY == 1)
                positionY = actualCellCountY - 1 - positionY;

            SetChildAlongAxis(rectChildren[i], 0, startOffset.x + (cellSize[0] + spacing[0]) * positionX, cellSize[0]);
            SetChildAlongAxis(rectChildren[i], 1, startOffset.y + (cellSize[1] + spacing[1]) * positionY, cellSize[1]);
        }
    }

}
