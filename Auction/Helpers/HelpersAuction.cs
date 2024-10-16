using Auction.Models.DBModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Xml;

namespace Auction.Helpers
{
    public class AuctionTimerTagHelper : TagHelper
    {
        public int IdLot { get; set; }
        public DateTime AuctionEndTime { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            TimeSpan timeLeft = AuctionEndTime - DateTime.Now;

            output.TagName = "div";

            string content = $@"
            <div class='auction-timer d-flex justify-content-around text-dark' data-idLot='{IdLot}' data-end-time='{AuctionEndTime.ToString("yyyy-MM-ddTHH:mm:ss")}'>
                <div class='bg-warning p-1 rounded text-center flex-fill mx-1'>
                    <span class='fs-5 d-block days'>{timeLeft.Days}</span>
                    days
                </div>
                <div class='bg-warning p-2 rounded text-center flex-fill mx-1'>
                    <span class='fs-5 d-block hrs'>{timeLeft.Hours}</span>
                    hrs
                </div>
                <div class='bg-warning p-2 rounded text-center flex-fill mx-1'>
                    <span class='fs-5 d-block min'>{timeLeft.Minutes}</span>
                    min
                </div>
                <div class='bg-warning p-2 rounded text-center flex-fill mx-1'>
                    <span class='fs-5 d-block sec'>{timeLeft.Seconds}</span>
                    sec
                </div>
            </div>";

            output.Content.SetHtmlContent(content);
        }
    }
    public class LotCardTagHelper : TagHelper
    {
        public Lot Lot { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "card-container");

            // Создание контента для таймера аукциона
            var auctionTimerTagHelper = new AuctionTimerTagHelper
            {
                IdLot = Lot.LotId,
                AuctionEndTime = Lot.EndTime
            };

            var tagHelperContext = new TagHelperContext(
                tagName: "auction-timer",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: Guid.NewGuid().ToString()
            );

            var tagHelperOutput = new TagHelperOutput(
                "auction-timer",
                new TagHelperAttributeList(),
                (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
            );

            auctionTimerTagHelper.Process(tagHelperContext, tagHelperOutput);

            var auctionTimerHtml = tagHelperOutput.Content.GetContent();

            // Формируем HTML контент карточки
            var content = $@"
                <a href='/Auction/InfoLot/{Lot.LotId}' class='text-decoration-none lot' data-idLot='{Lot.LotId}'>
                    <div class='card border border-secondary m-2'>
                        <div class='position-relative'>
                            <img src='{Lot.ImgPath}' class='card-img-top rounded' alt='Auction Item' style='height: 200px; object-fit: cover;'>
                            <div class='position-absolute top-0 end-0 p-2 bg-warning text-dark rounded-start'>
                                <h5 class='card-title mb-0'>{Lot.Title}</h5>
                            </div>
                        </div>
                        <div class='card-body bg-light'>
                            <p class='card-text text-dark'>
                                <strong>Current Bid:</strong> <span class='fs-4 currentBid'>{Lot.CurrentPrice}$</span><br>
                                {auctionTimerHtml}
                            </p>
                        </div>
                        <div class='card-footer bg-dark'>
                            <p class='card-text text-white'>
                                <strong>Location:</strong> {Lot.Location}
                            </p>
                        </div>
                    </div>
                </a>";

            output.Content.SetHtmlContent(content);
        }
    }

    public class MessageTagHelper : TagHelper
    {
        public string UserLogin { get; set; }
        public string Message { get; set; }
        public string FontColor { get; set; }
        public DateTime DateTime { get; set; }
        public int? IdMessage { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (IdMessage == null) { return; }

            output.TagName = "li"; // Используем тег li для создания элемента списка
            output.Attributes.SetAttribute("class", "list-group-item d-flex justify-content-start align-items-center mb-2 border border-0");

            var span = new TagBuilder("span");
            span.AddCssClass($"alert alert-{FontColor} d-inline-block mb-4 me-1");

            var strong = new TagBuilder("strong");
            strong.InnerHtml.Append(UserLogin + ": ");

            var messageSpan = new TagBuilder("span");
            messageSpan.InnerHtml.Append(Message);

            // Форматирование даты и времени
            var timeSpan = new TagBuilder("span");
            timeSpan.AddCssClass("text-muted ms-2");
            timeSpan.InnerHtml.Append(DateTime.ToString("dd MMMM, HH:mm"));

            span.InnerHtml.AppendHtml(strong);
            span.InnerHtml.AppendHtml(messageSpan);
            span.InnerHtml.AppendHtml(timeSpan);

            output.Content.AppendHtml(span);
        }
    }

}
