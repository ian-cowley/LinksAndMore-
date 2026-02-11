using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace LinksAndMore.Controls;

public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
{
    public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
        nameof(ItemWidth), typeof(double), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
        nameof(ItemHeight), typeof(double), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double ItemWidth
    {
        get => (double)GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    private Size _extent = new(0, 0);
    private Size _viewport = new(0, 0);
    private Point _offset;

    public bool CanHorizontallyScroll { get; set; }
    public bool CanVerticallyScroll { get; set; }

    public double ExtentWidth => _extent.Width;
    public double ExtentHeight => _extent.Height;
    public double ViewportWidth => _viewport.Width;
    public double ViewportHeight => _viewport.Height;
    public double HorizontalOffset => _offset.X;
    public double VerticalOffset => _offset.Y;

    public ScrollViewer? ScrollOwner { get; set; }

    protected override Size MeasureOverride(Size availableSize)
    {
        var itemsControl = ItemsControl.GetItemsOwner(this);
        if (itemsControl == null)
        {
            return availableSize;
        }

        var itemCount = itemsControl.Items.Count;
        var itemSize = GetItemSize(availableSize);

        var resolvedWidth = double.IsInfinity(availableSize.Width) ? itemsControl.ActualWidth : availableSize.Width;
        if (resolvedWidth <= 0)
        {
            resolvedWidth = itemSize.Width;
        }

        var resolvedHeight = double.IsInfinity(availableSize.Height) ? itemsControl.ActualHeight : availableSize.Height;
        if (resolvedHeight <= 0)
        {
            resolvedHeight = itemSize.Height;
        }

        var viewportSize = new Size(resolvedWidth, resolvedHeight);
        UpdateScrollInfo(viewportSize, itemCount, itemSize);

        var firstIndex = GetFirstVisibleIndex(itemSize, itemCount);
        var lastIndex = GetLastVisibleIndex(itemSize, itemCount);

        if (ScrollOwner == null && double.IsInfinity(availableSize.Height))
        {
            firstIndex = 0;
            lastIndex = Math.Max(0, itemCount - 1);
        }

        CleanUpItems(firstIndex, lastIndex);

        var generator = ItemContainerGenerator;
        var startPos = generator.GeneratorPositionFromIndex(firstIndex);
        var childIndex = startPos.Offset == 0 ? startPos.Index : startPos.Index + 1;

        using (generator.StartAt(startPos, GeneratorDirection.Forward, true))
        {
            for (var itemIndex = firstIndex; itemIndex <= lastIndex; itemIndex++, childIndex++)
            {
                var child = generator.GenerateNext(out var newlyRealized) as UIElement;
                if (child == null)
                {
                    continue;
                }

                if (newlyRealized)
                {
                    if (childIndex >= InternalChildren.Count)
                    {
                        AddInternalChild(child);
                    }
                    else
                    {
                        InsertInternalChild(childIndex, child);
                    }

                    generator.PrepareItemContainer(child);
                }

                child.Measure(itemSize);
            }
        }

        return new Size(
            double.IsInfinity(availableSize.Width) ? _extent.Width : availableSize.Width,
            double.IsInfinity(availableSize.Height) ? _extent.Height : availableSize.Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var itemSize = GetItemSize(finalSize);
        var itemsPerRow = GetItemsPerRow(finalSize, itemSize);

        for (var i = 0; i < InternalChildren.Count; i++)
        {
            var child = InternalChildren[i];
            var index = ItemContainerGenerator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));
            if (index < 0)
            {
                continue;
            }

            var row = index / itemsPerRow;
            var column = index % itemsPerRow;

            var rect = new Rect(
                column * itemSize.Width - _offset.X,
                row * itemSize.Height - _offset.Y,
                itemSize.Width,
                itemSize.Height);

            child.Arrange(rect);
        }

        return finalSize;
    }

    private Size GetItemSize(Size availableSize)
    {
        var width = ItemWidth > 0 ? ItemWidth : availableSize.Width;
        var height = ItemHeight > 0 ? ItemHeight : availableSize.Height;
        return new Size(Math.Max(1, width), Math.Max(1, height));
    }

    private int GetItemsPerRow(Size availableSize, Size itemSize)
    {
        return Math.Max(1, (int)Math.Floor(availableSize.Width / itemSize.Width));
    }

    private void UpdateScrollInfo(Size availableSize, int itemCount, Size itemSize)
    {
        var itemsPerRow = GetItemsPerRow(availableSize, itemSize);
        var rows = (int)Math.Ceiling(itemCount / (double)itemsPerRow);

        _extent = new Size(itemsPerRow * itemSize.Width, rows * itemSize.Height);
        _viewport = availableSize;

        ScrollOwner?.InvalidateScrollInfo();
    }

    private int GetFirstVisibleIndex(Size itemSize, int itemCount)
    {
        var itemsPerRow = GetItemsPerRow(_viewport, itemSize);
        var firstRow = (int)Math.Floor(_offset.Y / itemSize.Height);
        return Math.Max(0, Math.Min(itemCount - 1, firstRow * itemsPerRow));
    }

    private int GetLastVisibleIndex(Size itemSize, int itemCount)
    {
        if (itemCount == 0)
        {
            return 0;
        }

        var itemsPerRow = GetItemsPerRow(_viewport, itemSize);
        var lastRow = (int)Math.Ceiling((_offset.Y + _viewport.Height) / itemSize.Height) - 1;
        var lastIndex = (lastRow + 1) * itemsPerRow - 1;
        return Math.Min(itemCount - 1, Math.Max(0, lastIndex));
    }

    private void CleanUpItems(int firstIndex, int lastIndex)
    {
        var generator = ItemContainerGenerator;
        for (var i = InternalChildren.Count - 1; i >= 0; i--)
        {
            var child = InternalChildren[i];
            var index = generator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));
            if (index < firstIndex || index > lastIndex)
            {
                generator.Remove(new GeneratorPosition(i, 0), 1);
                RemoveInternalChildRange(i, 1);
            }
        }
    }

    public void LineUp() => SetVerticalOffset(VerticalOffset - 16);
    public void LineDown() => SetVerticalOffset(VerticalOffset + 16);
    public void LineLeft() => SetHorizontalOffset(HorizontalOffset - 16);
    public void LineRight() => SetHorizontalOffset(HorizontalOffset + 16);
    public void PageUp() => SetVerticalOffset(VerticalOffset - ViewportHeight);
    public void PageDown() => SetVerticalOffset(VerticalOffset + ViewportHeight);
    public void PageLeft() => SetHorizontalOffset(HorizontalOffset - ViewportWidth);
    public void PageRight() => SetHorizontalOffset(HorizontalOffset + ViewportWidth);
    public void MouseWheelUp() => SetVerticalOffset(VerticalOffset - 48);
    public void MouseWheelDown() => SetVerticalOffset(VerticalOffset + 48);
    public void MouseWheelLeft() => SetHorizontalOffset(HorizontalOffset - 48);
    public void MouseWheelRight() => SetHorizontalOffset(HorizontalOffset + 48);

    public void SetHorizontalOffset(double offset)
    {
        if (offset < 0 || ViewportWidth >= ExtentWidth)
        {
            offset = 0;
        }
        else if (offset + ViewportWidth >= ExtentWidth)
        {
            offset = ExtentWidth - ViewportWidth;
        }

        _offset.X = offset;
        ScrollOwner?.InvalidateScrollInfo();
        InvalidateMeasure();
    }

    public void SetVerticalOffset(double offset)
    {
        if (offset < 0 || ViewportHeight >= ExtentHeight)
        {
            offset = 0;
        }
        else if (offset + ViewportHeight >= ExtentHeight)
        {
            offset = ExtentHeight - ViewportHeight;
        }

        _offset.Y = offset;
        ScrollOwner?.InvalidateScrollInfo();
        InvalidateMeasure();
    }

    public Rect MakeVisible(Visual visual, Rect rectangle)
    {
        var element = visual as UIElement;
        if (element == null || !IsAncestorOf(element))
        {
            return rectangle;
        }

        var container = element;
        var generator = ItemContainerGenerator as ItemContainerGenerator;
        var index = generator?.IndexFromContainer(container) ?? -1;
        if (index < 0)
        {
            return rectangle;
        }

        var itemSize = GetItemSize(new Size(ViewportWidth, ViewportHeight));
        var itemsPerRow = GetItemsPerRow(_viewport, itemSize);
        var row = index / itemsPerRow;

        var desiredOffset = row * itemSize.Height;
        if (desiredOffset < VerticalOffset)
        {
            SetVerticalOffset(desiredOffset);
        }
        else if (desiredOffset + itemSize.Height > VerticalOffset + ViewportHeight)
        {
            SetVerticalOffset(desiredOffset + itemSize.Height - ViewportHeight);
        }

        return rectangle;
    }
}
