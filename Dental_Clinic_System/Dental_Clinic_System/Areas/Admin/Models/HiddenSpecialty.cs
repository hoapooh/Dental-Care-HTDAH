namespace Dental_Clinic_System.Areas.Admin.Models
{
    public class HiddenSpecialtyService()
    {
        private readonly List<int> _hiddenSpeciatlies = new List<int>();

        public void HiddenSpeciatly(int id)
        {
            if (!_hiddenSpeciatlies.Contains(id))
                _hiddenSpeciatlies.Add(id);
        }

        public List<int> GetHiddenSpeciatly()
        {
            return _hiddenSpeciatlies;
        }
    }

}
