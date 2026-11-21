using AutoMapper;
using SistemaStockWeb.Models.Entities;
using SistemaStockWeb.Models.ViewModels.Movimiento;

namespace SistemaStockWeb.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Movimiento, MovimientoViewModel>();
    }
}