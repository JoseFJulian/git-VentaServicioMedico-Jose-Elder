using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VentaYServicoMedico.Models.ViewModels
{
    public class SubCategoryAndCategoryViewModel
    { 
        //para generar una lista de categorias.
        public IEnumerable<Category> CategoryList { get; set; }
        //instanciar un objeto de tipo SubCategory
        public SubCategory SubCategory { get; set; }
        //para generar una lista de SubCategory
        public List<string> SubCategoryList { get; set; }
        /*Nos servirá para desplegar un mensaje de éxito
            o de falla en el CRUD de SubCategory
            */
        public string StatusMessage { get; set; }
    }
}
