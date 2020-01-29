using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace ForkBot
{
    public class InfoEmbed
    {
        JEmbed emb;

        public Embed Build()
        {
            return emb.Build();
        }

        public InfoEmbed(string title, string msg, string footer = "", string image = Constants.Images.ForkBot)
        {
            emb = new JEmbed
            {
                Description = msg,
                ColorStripe = Constants.Colours.YORK_RED,
                Title = title,
                ThumbnailUrl = image,
                Footer = new JEmbedFooter(footer)
            };
        }
    }
}
