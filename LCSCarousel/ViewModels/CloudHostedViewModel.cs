﻿using LCSCarousel;
using LCSCarousel.Classes;
using LCSCarousel.Enums;
using LCSCarousel.Model;
using LCSCarousel.Mvvm;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LCSCarousel.ViewModels
{
    public class CloudHostedViewModel : BindableBase
    {
        private System.Collections.ObjectModel.ObservableCollection<Model.RDPTerminal> _RDPTerminals;
        private Model.RDPTerminal _selectedRDPTerminal;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")]
        public System.Collections.ObjectModel.ObservableCollection<Model.RDPTerminal> RDPTerminals
        {
            get { return _RDPTerminals; }
            set { this.SetProperty(ref this._RDPTerminals, value); }
        }
        public Model.RDPTerminal SelectedRDPTerminal
        {
            get { return _selectedRDPTerminal; }
            set { this.SetProperty(ref this._selectedRDPTerminal, value); }
        }
        public int NumberOfMachines { private set; get; }

        public CloudHostedViewModel()
        {
            RDPTerminals = new System.Collections.ObjectModel.ObservableCollection<Model.RDPTerminal>();

            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            List<CloudHostedInstance> cloudHostedTerminals = mainWindow.CloudHosted;
            List<InstanceAttribute> instanceAttributes = mainWindow.InstanceAttributes;
            mainWindow.EnableCloudHosted(false);


            FilterValues filterValues = mainWindow.GetFilter(CloudEnvironment.CloudHosted);

            NumberOfMachines = cloudHostedTerminals.Count;

            if (cloudHostedTerminals != null)
            {
                foreach (CloudHostedInstance instance in cloudHostedTerminals)
                {
                    InstanceAttribute instanceAttribute = instanceAttributes.Find(x => x.EnvironmentId == instance.EnvironmentId);

                    string imageSource = "/Resources/DefaultVM.png";
                    if (instanceAttribute != null)
                    {
                        imageSource = instanceAttribute.ImageSource;
                    }

                    EnvironmentState envState = new EnvironmentState()
                    { 
                        StateDescription = instance.DeploymentStatus,
                        StateNum = instance.DeploymentState
                    };
                    mainWindow.AddEnvironmentState(envState);

                    PlatformReleaseInformation platformReleaseInformation = new PlatformReleaseInformation()
                    {
                        PlatformRelease = instance.CurrentPlatformReleaseName
                    };

                    ReleaseInformation releaseinformation = new ReleaseInformation()
                    { 
                        Release = instance.CurrentApplicationReleaseName
                    };

                    if(platformReleaseInformation.PlatformRelease != null && releaseinformation.Release != null)
                    {
                        mainWindow.AddReleaseInformation(releaseinformation, platformReleaseInformation);
                    }

                    bool okToAdd = false;
                    if (filterValues.Active == true)
                    {
                        if (instance.DeploymentStatus == filterValues.environmentState.StateDescription
                            && instance.CurrentApplicationReleaseName == filterValues.releaseInformation.Release
                            && instance.CurrentPlatformReleaseName == filterValues.platformReleaseInformation.PlatformRelease)
                        {
                            okToAdd = true;
                        }
                    
                    }
                    else
                    {
                        okToAdd = true;
                    }

                    if (okToAdd == true)
                    {
                        RDPTerminals.Add(new Model.RDPTerminal()
                        {
                            InstanceId = instance.InstanceId,
                            DeploymentStatus = instance.DeploymentStatus,
                            ApplicationRelease = instance.CurrentApplicationReleaseName,
                            CurrentPlatformReleaseName = instance.CurrentPlatformReleaseName,
                            TopologyType = instance.TopologyType,
                            DisplayName = instance.DisplayName,
                            EnvironmentId = instance.EnvironmentId,
                            Instances = instance.Instances,
                            SqlAzureCredentials = instance.SqlAzureCredentials,
                            NavigationLinks = instance.NavigationLinks,
                            ImageSource = imageSource
                        });
                    }

                }
                if(RDPTerminals.Count > 0)
                {
                    RDPTerminals.BubbleSort();
                    SelectedRDPTerminal = RDPTerminals[0];
                    mainWindow.EnableCloudHosted(true);
                }
            }

        }

        // Delete the selected item
        public void Delete()
        {
            RDPTerminals.Remove(SelectedRDPTerminal);
            SelectedRDPTerminal = RDPTerminals[0];
        }
        public void ResetImage()
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            List<InstanceAttribute> instanceAttributes = mainWindow.InstanceAttributes;

            InstanceAttribute instanceAttribute = instanceAttributes.Find(x => x.EnvironmentId == SelectedRDPTerminal.EnvironmentId);

            if (instanceAttribute != null)
            {
                instanceAttributes.Remove(instanceAttribute);
            }

            Properties.Settings.Default.instanceattributes = JsonConvert.SerializeObject(instanceAttributes, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            Properties.Settings.Default.Save();

            var changedVM = new Model.RDPTerminal()
            {
                InstanceId = SelectedRDPTerminal.InstanceId,
                DeploymentStatus = SelectedRDPTerminal.DeploymentStatus,
                ApplicationRelease = SelectedRDPTerminal.ApplicationRelease,
                CurrentPlatformReleaseName = SelectedRDPTerminal.CurrentPlatformReleaseName,
                TopologyType = SelectedRDPTerminal.TopologyType,
                DisplayName = SelectedRDPTerminal.DisplayName,
                EnvironmentId = SelectedRDPTerminal.EnvironmentId,
                Instances = SelectedRDPTerminal.Instances,
                SqlAzureCredentials = SelectedRDPTerminal.SqlAzureCredentials,
                NavigationLinks = SelectedRDPTerminal.NavigationLinks,
                ImageSource = Properties.Settings.Default.DefaultImage
            };

            this.Delete();

            RDPTerminals.Add(changedVM);
            RDPTerminals.BubbleSort();
            SelectedRDPTerminal = changedVM;

        }
        public void ChangeImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))return;

            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            List<InstanceAttribute> instanceAttributes = mainWindow.InstanceAttributes;

            InstanceAttribute instanceAttribute = instanceAttributes.Find(x => x.EnvironmentId == SelectedRDPTerminal.EnvironmentId);

            if (instanceAttribute != null)
            {
                instanceAttribute.ImageSource = imagePath;
            }
            else 
            {
                instanceAttributes.Add(new InstanceAttribute() { EnvironmentId = SelectedRDPTerminal.EnvironmentId, ImageSource = imagePath }); 

            }

            Properties.Settings.Default.instanceattributes = JsonConvert.SerializeObject(instanceAttributes, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            Properties.Settings.Default.Save();

            var changedVM = new Model.RDPTerminal() 
            { 
                InstanceId = SelectedRDPTerminal.InstanceId, 
                DeploymentStatus = SelectedRDPTerminal.DeploymentStatus, 
                ApplicationRelease = SelectedRDPTerminal.ApplicationRelease,
                CurrentPlatformReleaseName = SelectedRDPTerminal.CurrentPlatformReleaseName, 
                TopologyType = SelectedRDPTerminal.TopologyType, 
                DisplayName = SelectedRDPTerminal.DisplayName,
                EnvironmentId = SelectedRDPTerminal.EnvironmentId,
                Instances = SelectedRDPTerminal.Instances,
                SqlAzureCredentials = SelectedRDPTerminal.SqlAzureCredentials,
                NavigationLinks = SelectedRDPTerminal.NavigationLinks,
                ImageSource = imagePath};

            this.Delete();

            RDPTerminals.Add(changedVM);
            RDPTerminals.BubbleSort();
            SelectedRDPTerminal = changedVM;
        }

    }
}
