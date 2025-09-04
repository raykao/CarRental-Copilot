﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using CarRental.Controllers.Shared;
using CarRental.Domain.VehicleImageModule;

namespace CarRental.Controllers.VehicleImageModule
{
    public class VehicleImageController : Controller<VehicleImage>
    {
        #region Queries
        private const string insertCommand = @"INSERT INTO [Vehicle_Image] 
                                                (
                                                 [VehicleId],
                                                 [Image]
                                                )VALUES
                                                (
                                                @VehicleId,
                                                @Image
                                                );";
        private const string deleteCommand = "DELETE FROM [Vehicle_Image] WHERE [Id] = @Id";
        private const string deleteAllByVehicleIdCommand = "DELETE FROM [Vehicle_Image] WHERE [VehicleId] = @VehicleId";
        private const string selectAllByVehicleIdCommand = "SELECT * FROM [Vehicle_Image] WHERE [VehicleId] = @VehicleId;";
        private const string selectByIdCommand = "SELECT * FROM [Vehicle_Image] WHERE [Id] = @Id";
        private const string selectByVehicleIdCommand = "SELECT * FROM [Vehicle_Image] WHERE [VehicleId] = @VehicleId";
        private const string selectAllCommand = "SELECT * FROM [Vehicle_Image]";
        #endregion
        public override string Edit(int id, VehicleImage record)
        {
            record.Id = Db.Insert(insertCommand, GetImageParameters(record));
            return "";
        }

        public void EditList(List<VehicleImage> records)
        {
            if (records != null)
            {
                if (records.Count != 0)
                    DeleteByVehicleId(records[0].VehicleId);
                foreach (VehicleImage image in records)
                {
                    InsertNew(image);
                }
            }
        }

        public override bool Delete(int id)
        {
            try
            {
                Db.Delete(deleteCommand, AddParameter("Id", id));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
        public bool DeleteByVehicleId(int vehicleId)
        {
            try
            {
                Db.Delete(deleteAllByVehicleIdCommand, AddParameter("VehicleId", vehicleId));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public override bool Exists(int id)
        {
            throw new NotImplementedException();
        }

        public override string InsertNew(VehicleImage record)
        {
            string validationResult = "VALID";

            record.Id = Db.Insert(insertCommand, GetImageParameters(record));

            return validationResult;
        }

        public override VehicleImage SelectById(int id)
        {
            return Db.Get(selectByIdCommand, ConvertToVehicleImage, AddParameter("Id", id));
        }
        public List<VehicleImage> SelectByVehicleId(int vehicleId)
        {
            return Db.GetAll(selectByVehicleIdCommand, ConvertToVehicleImage, AddParameter("VehicleId", vehicleId));
        }

        public override List<VehicleImage> SelectAll()
        {
            return Db.GetAll(selectAllCommand, ConvertToVehicleImage);
        }

        public List<VehicleImage> SelectAllImagesOfVehicle(int vehicleId)
        {
            return Db.GetAll(selectAllByVehicleIdCommand, ConvertToVehicleImage, AddParameter("VehicleId", vehicleId));
        }

        private Dictionary<string, object> GetImageParameters(VehicleImage vehicleImage)
        {
            Image<Rgba32> image = vehicleImage.Image;
            MemoryStream memory = new MemoryStream();
            image.Save(memory, new BmpEncoder());
            byte[] imageBytes = memory.ToArray();

            var parameters = new Dictionary<string, object>();

            parameters.Add("Id", vehicleImage.Id);
            parameters.Add("VehicleId", vehicleImage.VehicleId);
            parameters.Add("Image", imageBytes);

            return parameters;
        }

        private Image<Rgba32> ConvertToImage(IDataReader reader)
        {
            byte[] bytes = (byte[])(reader["Image"]);
            
            MemoryStream stream = new MemoryStream(bytes);
            Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(stream);

            return image;
        }

        private VehicleImage ConvertToVehicleImage(IDataReader reader)
        {
            byte[] byteArray = (byte[])(reader["Image"]);
            var id = Convert.ToInt32(reader["Id"]);
            var vehicleId = Convert.ToInt32(reader["VehicleId"]);

            MemoryStream stream = new MemoryStream(byteArray);
            Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(stream);

            return new VehicleImage(id, vehicleId, image);
        }
    }
}
