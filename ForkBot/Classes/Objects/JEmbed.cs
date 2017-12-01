using System;
using System.Collections.Generic;
using Discord;


namespace ForkBot
{
    /// <summary>
    /// A class for creating and building Discord.NET embeds.
    /// </summary>
    // Makes creating embeds less garbage.
    public class JEmbed
    {
        /// <summary>
        /// Author information that appears at the top of the embed.
        /// If no author is specified then the author information does not appear.
        /// </summary>
        public JEmbedAuthor Author { get; set; }

        /// <summary>
        /// Footer that appears at the bottom of the embed.
        /// If no footer is specified then the footer does not appear.
        /// </summary>
        public JEmbedFooter Footer { get; set; }

        /// <summary>
        /// Title of the embed.
        /// Appears below the author field if one is present.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// URL that acts as a hyperlink for the title.
        /// If no url is specified then the title will not be a hyperlink.
        /// </summary>
        public string TitleUrl { get; set; }

        /// <summary>
        /// Description of the embed. Appears below the title field.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Thumbnail of the embed.
        /// Appears at the top right of the embed.
        /// The image is automatically scaled down.
        /// If no image url is specified then an image does not appear.
        /// </summary>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// List of fields that appear in the embed.
        /// </summary>
        public List<JEmbedField> Fields { get; private set; }

        /// <summary>
        /// Color of the stripe that lines the left side of the embed.
        /// </summary>
        public Color? ColorStripe { get; set; }

        /// <summary>
        /// Timestamp that can appear at the foot of the embed.
        /// Appears to the right of the footer if one is present.
        /// </summary>
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// URL of an image that will be displayed inside the embed.
        /// Appears below the description and any fields specified.
        /// The image is automatically scaled to match the embed's width.
        /// If no image url is specified then an image does not appear.
        /// </summary>
        public string ImageUrl { get; set; }

        public JEmbed()
        {
            Author = new JEmbedAuthor();
            Footer = new JEmbedFooter();
            Fields = new List<JEmbedField>();
        }

        private bool IsUrl(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) { return false; }

            return Uri.TryCreate(str, UriKind.Absolute, out Uri uriResult)
               && (uriResult.Scheme == "http" || uriResult.Scheme == "https");
        }

        /// <summary>
        /// Builds the embed and returns a Discord.Net embed object.
        /// </summary>
        /// <param name="currentTimestamp">Determines whether the timestamp parameter should be set to the current UTC time.</param>
        /// <returns>A Discord.Net embed object.</returns>
        public Embed Build(bool currentTimestamp = false)
        {
            if (currentTimestamp) { this.Timestamp = DateTimeOffset.UtcNow; }

            EmbedBuilder emb = new EmbedBuilder();

            // Title and Url.
            if (!string.IsNullOrWhiteSpace(Title))
            {
                emb.WithTitle(Title);
                if (IsUrl(TitleUrl))
                {
                    emb.WithUrl(TitleUrl);
                }
            }

            // Description.
            if (!string.IsNullOrWhiteSpace(Description))
            {
                emb.WithDescription(Description);
            }

            // Author embed.
            EmbedAuthorBuilder embAuth = Author.Build();
            if (embAuth != null)
            {
                emb.WithAuthor(embAuth);
            }

            // Color.
            if (ColorStripe != null)
            {
                emb.WithColor((Color)ColorStripe);
            }

            // Footer embed.
            EmbedFooterBuilder embFoot = Footer.Build();
            if (embFoot != null)
            {
                emb.WithFooter(embFoot);
            }

            // Field embeds.
            foreach (JEmbedField fld in Fields)
            {
                EmbedFieldBuilder embFld = fld.Build();
                if (embFld != null)
                {
                    emb.AddField(embFld);
                }
            }

            // Thumbnail.
            if (IsUrl(ThumbnailUrl))
            {
                emb.WithThumbnailUrl(ThumbnailUrl);
            }

            // Image.
            if (IsUrl(ImageUrl))
            {
                emb.WithImageUrl(ImageUrl);
            }

            // Timestamp.
            if (Timestamp != null)
            {
                emb.WithTimestamp((DateTimeOffset)Timestamp);
            }

            return emb.Build();
        }
    }

    /// <summary>
    /// Class used to display author information at the top of an embed.
    /// </summary>
    public class JEmbedAuthor
    {
        /// <summary>
        /// Name of the embed's author. Displayed at the top of the embed.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// URL of an image that will be displayed to the left of the author name.
        /// The image is automatically scaled to match the author name's font height.
        /// If no image url is specified then an image does not appear.
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// URL that acts as a hyperlink for the author name.
        /// If no url is specified then the author name will not be a hyperlink.
        /// </summary>
        public string Url { get; set; }

        public JEmbedAuthor() : this(string.Empty, string.Empty, string.Empty) { }

        public JEmbedAuthor(string name, string iconUrl = "", string url = "")
        {
            this.Name = name;
            this.IconUrl = iconUrl;
            this.Url = url;
        }

        public JEmbedAuthor(Action<JEmbedAuthor> action)
        {
            action(this);
        }

        private bool IsUrl(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) { return false; }

            return Uri.TryCreate(str, UriKind.Absolute, out Uri uriResult)
               && (uriResult.Scheme == "http" || uriResult.Scheme == "https");
        }

        /// <summary>
        /// Builds the author embed and returns a Discord.Net embed author object.
        /// </summary>
        /// <returns>A Discord.Net embed author object.</returns>
        public EmbedAuthorBuilder Build()
        {
            if (string.IsNullOrWhiteSpace(Name)) { return null; }

            EmbedAuthorBuilder embAuth = new EmbedAuthorBuilder().WithName(Name);
            if (IsUrl(Url))
            {
                embAuth.WithUrl(Url);
            }
            if (IsUrl(IconUrl))
            {
                embAuth.WithIconUrl(IconUrl);
            }

            return embAuth;
        }
    }

    /// <summary>
    /// A footer that can be added to the bottom of an embed.
    /// </summary>
    public class JEmbedFooter
    {
        /// <summary>
        /// The string of text to appear in the footer.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The url of an image that will appear to the left of the footer text.
        /// The image is automatically scaled to match the footer text's font.
        /// If no image url is specified then an image does not appear.
        /// </summary>
        public string IconUrl { get; set; }

        public JEmbedFooter() : this(string.Empty, string.Empty) { }

        public JEmbedFooter(string text, string iconUrl = "")
        {
            this.Text = text;
            this.IconUrl = iconUrl;
        }

        public JEmbedFooter(Action<JEmbedFooter> action)
        {
            action(this);
        }

        private bool IsUrl(string str)
        {

            return Uri.TryCreate(str, UriKind.Absolute, out Uri uriResult)
               && (uriResult.Scheme == "http" || uriResult.Scheme == "https");
        }

        /// <summary>
        /// Builds the footer embed and returns a Discord.Net embed footer object.
        /// </summary>
        /// <returns>A Discord.Net embed footer object.</returns>
        public EmbedFooterBuilder Build()
        {
            if (string.IsNullOrWhiteSpace(Text) && string.IsNullOrWhiteSpace(IconUrl)) { return null; }

            EmbedFooterBuilder embFoot = new EmbedFooterBuilder();
            embFoot.WithText(Text);

            if (IsUrl(IconUrl))
            {
                embFoot.WithIconUrl(IconUrl);
            }

            return embFoot;
        }
    }

    /// <summary>
    /// A set of a header and text that can appear in the main body of an embed.
    /// If the header or text properties are empty then it will not show up in an embed when built.
    /// </summary>
    public class JEmbedField
    {
        /// <summary>
        /// Determines whether the field can share a line with other fields.
        /// </summary>
        public bool Inline { get; set; }

        /// <summary>
        /// Header of a field. Is it automatically bolded.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// The main text of a field.
        /// </summary>
        public string Text { get; set; }

        public JEmbedField() : this(string.Empty, string.Empty) { }

        public JEmbedField(string header, string text, bool inline = true)
        {
            this.Header = header;
            this.Text = text;
            this.Inline = inline;
        }

        public JEmbedField(Action<JEmbedField> action)
        {
            action(this);
        }

        public EmbedFieldBuilder Build()
        {
            if (string.IsNullOrWhiteSpace(Text) || string.IsNullOrWhiteSpace(Header)) { return null; }

            return new EmbedFieldBuilder().WithName(Header).WithValue(Text).WithIsInline(Inline);
        }
    }
}