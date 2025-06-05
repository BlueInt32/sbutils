// See https://aka.ms/new-console-template for more information

using SbuTils.Processing;

// var mp4path =
//     "/mnt/WIN_E/save-billy/_backups/Sexart/sexart.17.05.26.alexis.crystal.and.belle.claire.me.and.myself.part.2.mp4";
// var report = await ShellHelper.RunCommand(
//     "ffprobe",
//     $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 {mp4path}"
// );
//
// // report.FancyReportEnumerator(Console.WriteLine);
// Console.WriteLine(report.FancyReport);

var ffmpegReport = await ShellHelper.RunCommand(
    "ffmpeg",
    "-discard nokey -i \"/home/simon/dev/vstack-v3/integration-testing/dummy-catalogs/static-used-in-postman-tests/_sub1/sc3.mp4\" -vf select=\"lt(selected_n\\,20)*(isnan(prev_selected_t)+gte(t-prev_selected_t\\,0))\",scale=500:-1 -vsync vfr -y -f image2pipe -vcodec ppm -"
);
Console.WriteLine(ffmpegReport.FancyReport);
