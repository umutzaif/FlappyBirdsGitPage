using Microsoft.AspNetCore.Mvc;
using EnergyMonitor.Business;
using System.Threading.Tasks;

namespace EnergyMonitor.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly BAnaliz _business;

        public AnalysisController()
        {
            // In real app, use DI and configuration
            var connStr = "Host=localhost;Database=energymonitordb;Username=postgres;Password=admin";
            _business = new BAnaliz(connStr);
        }

        [HttpGet("prediction")]
        public async Task<IActionResult> GetPrediction()
        {
            var result = await _business.AylikTuketimTahminEtAsync();
            return Ok(new { MonthlyPredictionKwh = result });
        }

        [HttpGet("bill-estimation")]
        public async Task<IActionResult> GetBillEstimation()
        {
            var result = await _business.FaturaTahminEtAsync();
            return Ok(new { EstimatedBillTL = result });
        }

        [HttpGet("recommendation")]
        public async Task<IActionResult> GetRecommendation()
        {
            var result = await _business.TasarrufOnerisiGetirAsync();
            return Ok(new { Recommendation = result });
        }
    }
}
