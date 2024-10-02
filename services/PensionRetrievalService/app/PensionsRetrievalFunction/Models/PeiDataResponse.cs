namespace PensionsRetrievalFunction.Models;

public class PeiDataResponse
{
    private readonly Dictionary<string, PeiData> _peiData = [];

    public PeiDataResponse(string? rpt, IEnumerable<PeiData> peis)
    {
        AddPeiRange(peis);
        SetRpt(rpt);
    }

    public string? Rpt { get; private set; }

    public void SetRpt(string? rpt)
    {
        if (!string.IsNullOrEmpty(rpt))
        {
            Rpt = rpt;
        }
    }

    public void AddPeiRange(IEnumerable<PeiData> peis)
    {
        if (peis == null) return;
        peis.ToList().ForEach(pei => _peiData.TryAdd(pei.Pei, pei));
    }

    public bool TryAdd(PeiData pei)
    {
        if(pei == null) return false;
        return _peiData.TryAdd(pei.Pei, pei);
    }

    public IReadOnlyCollection<PeiData> PeiData => _peiData.Values;
}
