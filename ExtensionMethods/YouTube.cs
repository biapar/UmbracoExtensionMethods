using System;
using System.Text.RegularExpressions;

namespace Umbraco.Community.ExtensionsMethods {
    
    public static class YouTube {

        /// <summary>
        /// Uses regular expressions for finding a YouTube video ID in the string.
        /// </summary>
        /// <param name="subject">The string to search.</param>
        /// <returns>The YouTube video ID if found, otherwise <var>NULL</var>.</returns>
        public static string GetYouTubeId(this string subject) {
            Match m1 = Regex.Match(subject, "^((\\w|-){11}$)");
            Match m2 = Regex.Match(subject, "v=((\\w|-){11})");
            Match m3 = Regex.Match(subject, "\\/((\\w|-){11})$");
            return (m1.Success ? m1.Groups[1].Value : (m2.Success ? m2.Groups[1].Value : (m3.Success ? m3.Groups[1].Value : null)));
        }

        /// <summary>
        /// Uses regular expressions for finding a YouTube video ID in the string.
        /// </summary>
        /// <param name="subject">The string to search.</param>
        /// <param name="videoId">The YouTube video ID if found, otherwise <var>NULL</var>.</param>
        /// <returns>Returns <var>TRUE</var> if a video ID is found, otherwise <var>FALSE</var>.</returns>
        public static bool GetYouTubeId(this string subject, out string videoId) {
            
            Match m1 = Regex.Match(subject, "^((\\w|-){11}$)");
            Match m2 = Regex.Match(subject, "v=((\\w|-){11})");
            Match m3 = Regex.Match(subject, "\\/((\\w|-){11})$");

            if (m1.Success) {
                videoId = m1.Groups[1].Value;
                return true;
            }

            if (m2.Success) {
                videoId = m2.Groups[1].Value;
                return true;
            }

            if (m3.Success) {
                videoId = m3.Groups[1].Value;
                return true;
            }

            videoId = null;
            return false;
        
        }

        /// <summary>
        /// Builds the HTML embed iframe for the specified video.
        /// </summary>
        /// <param name="videoId">The YouTube ID of the video.</param>
        /// <param name="width">The desired width of the iframe.</param>
        /// <param name="height">The desired height of the iframe.</param>
        public static string YouTubeEmbed(this string videoId, int width, int height) {
            if (!Regex.IsMatch(videoId, "^[\\w|-]{11}$")) return null;
            return String.Format(
                "<iframe src=\"{0}\" width=\"{1}\" height=\"{2}\" frameborder=\"0\" allowfullscreen></iframe>",
                "http://www.youtube.com/embed/" + videoId,
                width,
                height
            );
        }

        /// <summary>
        /// Builds the HTML embed iframe for the specified video.
        /// </summary>
        /// <param name="videoId">The YouTube ID of the video.</param>
        /// <param name="width">The desired width of the iframe.</param>
        /// <param name="height">The desired height of the iframe.</param>
        /// <param name="showRelations">By the default, YouTube will show
        /// related videos at the end of videos. Setting this to
        /// <var>FALSE</var> will disable the feature.</param>
        /// <param name="wmode">The flash video player doesn't really
        /// play well with layers (mostly in IE). Setting this
        /// parameter to <var>transarent</var> will solve most
        /// of these issues.</param>
        public static string YouTubeEmbed(this string videoId, int width, int height, bool showRelations, string wmode) {
            if (!Regex.IsMatch(videoId, "^[\\w|-]{11}$")) return null;
            return String.Format(
                "<iframe src=\"{0}\" width=\"{1}\" height=\"{2}\" frameborder=\"0\" allowfullscreen></iframe>",
                "http://www.youtube.com/embed/" + videoId + "?rel=" + (showRelations ? 1 : 0) + "&wmode=" + wmode,
                width,
                height
            );
        }

        /// <summary>
        /// Gets the default thumbnail URL for a video with the specified ID. The default
        /// thumbnail measures 480x360 pixels.
        /// </summary>
        /// <param name="videoId">The ID of the video.</param>
        /// <returns>The thumbnail URL if the video ID is valid, otherwise <var>NULL</var>.</returns>
        public static string GetYouTubeThumbnail(this string videoId) {
            return GetYouTubeThumbnail(videoId, 0);
        }

        /// <summary>
        /// Gets the thumbnail URL for a video with the specified ID. The default thumbnail (index = 0)
        /// measures 480x360 pixels, while the others measures 120x90 pixels.
        /// </summary>
        /// <param name="videoId">The ID of the video.</param>
        /// <param name="index">The index of the thumbnail URL to return
        /// (valid range is from 0 to 3 - both inclusive).</param>
        /// <returns>The thumbnail URL if the video ID is valid, otherwise <var>NULL</var>.</returns>
        public static string GetYouTubeThumbnail(this string videoId, int index) {
            if (!Regex.IsMatch(videoId, "^[\\w|-]{11}$")) return null;
            return "http://i.ytimg.com/vi/" + videoId + "/" + index + ".jpg";
        }

    }

}