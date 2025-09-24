using Content.Goobstation.Common.StationReport;
using Content.Server.GameTicking;
using Content.Shared.Paper;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.StationReportSystem;

public sealed class StationReportSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!; // Omu

    public override void Initialize()
    {
        //subscribes to the endroundevent
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend);
    }

    private void OnRoundEndTextAppend(RoundEndTextAppendEvent args)
    {
        var reportDefaultFormLoc = ((PaperComponent) _prototypeManager.Index("NanoRepStationReport").Components["paper"].Component).Content; // Omu: Don't send a report that hasn't been filled in

        //locates the first entity with StationReportComponent then stops
        string? stationReportText = null;
        var query = EntityQueryEnumerator<StationReportComponent>();
        while (query.MoveNext(out var uid, out var tablet))//finds the first entity with stationreport
        {
            if (!TryComp<PaperComponent>(uid, out var paper))
                return;

            stationReportText = paper.Content;
            break;
        }

        if(stationReportText != Loc.GetString(reportDefaultFormLoc)) // Omu: Don't send a report that hasn't been filled in
            BroadcastStationReport(stationReportText);
    }

    //sends a networkevent to tell the client to update the stationreporttext when recived
    public void BroadcastStationReport(string? stationReportText)
    {
        RaiseNetworkEvent(new StationReportEvent(stationReportText));//to send to client
        RaiseLocalEvent(new StationReportEvent(stationReportText));//to send to discord intergration
    }
}
