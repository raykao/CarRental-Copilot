using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarRental.Domain.Shared;
using CarRental.Domain.VehicleModule;
using System.IO;

namespace CarRental.Domain.VehicleImageModule
{
    public class VehicleImage : BaseEntity
    {
        public int VehicleId { get; set; }
        public Image<Rgba32> Image { get; set; }

        public VehicleImage() { }
        public VehicleImage(int id, int vehicleId, Image<Rgba32> image)
        {
            this.id = id;
            this.VehicleId = vehicleId;
            this.Image = image;
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }

        public override string Validate()
        {
            string validationResult = "VALID";
            return validationResult;
        }

        public override bool Equals(object obj)
        {
            return obj is VehicleImage vehicleImage &&
                   EqualityComparer<int>.Default.Equals(this.VehicleId, vehicleImage.VehicleId) &&
                   EqualityComparer<Image<Rgba32>>.Default.Equals(Image, vehicleImage.Image) &&
                   EqualityComparer<int>.Default.Equals(Id, vehicleImage.id);
        }

        public override int GetHashCode()
        {
            int hashCode = 155997214;
            hashCode = hashCode * -1521134295 + id.GetHashCode();
            hashCode = hashCode * -1521134295 + VehicleId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Image<Rgba32>>.Default.GetHashCode(Image);
            return hashCode;
        }
    }
}
