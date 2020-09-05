using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorTable
{
    public class TableBase<Item>:ComponentBase
    {
        public int pagesCount;
        public int curPage;
        public int pagerSize;
        public int startPage;
        public int endPage;
        [Parameter]
        public RenderFragment TableHeader { get; set; }
        [Parameter]
        public RenderFragment<Item> TableRow { get; set; }
        [Parameter]
        public IEnumerable<Item> Items { get; set; }
        [Parameter]
        public int PageSize { get; set; }
        public IEnumerable<Item> ItemList { get; set; }
        protected override async Task OnInitializedAsync()
        {
            pagerSize = 6;
            curPage = 1;
            ItemList = Items.Skip((curPage - 1) * PageSize).Take(PageSize);
            pagesCount = (int)Math.Ceiling(Items.Count() / (decimal)PageSize);
            SetPagerSize("forward");
        }
        public void updateList(int currentPage)
        {
            ItemList = Items.Skip((currentPage - 1) * PageSize).Take(PageSize);
            curPage = currentPage;
            StateHasChanged();
        }
        public void SetPagerSize(string direction)
        {
            if (direction == "forward" && endPage < pagesCount)
            {
                startPage = endPage + 1;
                if (endPage + pagerSize < pagesCount)
                {
                    endPage = startPage + pagerSize - 1;
                }
                else
                {
                    endPage = pagesCount;
                }
                StateHasChanged();
            }
            else if (direction == "back" && startPage > 1)
            {
                endPage = startPage - 1;
                startPage = startPage - pagerSize;
            }
        }
        public void NavigateToPage(string direction)
        {
            if (direction == "next")
            {
                if (curPage < pagesCount)
                {
                    if (curPage == endPage)
                    {
                        SetPagerSize("forward");
                    }
                    curPage += 1;
                }
            }
            else if (direction == "previous")
            {
                if (curPage > 1)
                {
                    if (curPage == startPage)
                    {
                        SetPagerSize("back");
                    }
                    curPage -= 1;
                }
            }
            updateList(curPage);
        }
    }
}
