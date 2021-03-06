﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IMSLogicLayer.ServiceInterfaces;
using IMSLogicLayer.Services;
using InterventionManagementSystem_MVC.Models;
using Microsoft.AspNet.Identity;
using InterventionManagementSystem_MVC.Areas.SiteEngineer.Models;
using IMSLogicLayer.Models;
using IMSLogicLayer.Enums;

namespace InterventionManagementSystem_MVC.Areas.SiteEngineer.Controllers
{
    [SiteEngineerAuthorize]
    [HandleError]
    public class SiteEngineerController : Controller
    {
        private IEngineerService engineer;

        private IEngineerService Engineer
        {
            get
            {
                if (engineer == null)
                {
                    engineer = new EngineerService(System.Web.HttpContext.Current.User.Identity.GetUserId());
                }
                return engineer;
            }
        }

        public SiteEngineerController() { }

        public SiteEngineerController(IEngineerService engineer)
        {
            this.engineer = engineer;
        }


        /// <summary>
        /// Display engineer details
        /// GET: ~/SiteEngineer/SiteEngineer
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var user = Engineer.getDetail();
            var model = new SiteEngineerViewModel()
            {
                Name = user.Name,
                DistrictName = user.District.Name,
                AuthorisedCosts = user.AuthorisedCosts,
                AuthorisedHours = user.AuthorisedHours
            };
            return View(model);
        }
        /// <summary>
        /// Display the create client form
        /// GET: SiteEngineer/SiteEngineer/CreateClient
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateClient()
        {
            ClientViewModel clientViewmodel = new ClientViewModel()
            {
                DistrictName = Engineer.getDetail().District.Name
            };
            return View(clientViewmodel);
        }


        /// <summary>
        /// Create the client
        /// POST: ~/SiteEngineer/SiteEngineer/CreateClient
        /// </summary>
        /// <param name="clientVmodel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult CreateClient(ClientViewModel clientVmodel)/*([Bind(Include = "Id,Name,Description,Length,Price,Rating,IncludesMeals")] Tour tour)*/
        {
            if (ModelState.IsValid)
            {
                Client client = Engineer.createClient(clientVmodel.Name, clientVmodel.Location);
                var clientList = Engineer.getClients();
                var clients = new List<ClientViewModel>();
                BindClient(clientList, clients);
                return View("ClientList", clients);
            }
            return View(clientVmodel);
        }


        /// <summary>
        /// Display a list of client
        /// GET: ~/SiteEngineer/SiteEngineer/ClientList
        /// </summary>
        /// <returns></returns>
        public ActionResult ClientList()
        {

            var clientList = Engineer.getClients();
            List<ClientViewModel> clients = new List<ClientViewModel>();
            BindClient(clientList, clients);
            return View(clients);
        }


        /// <summary>
        /// Display the create intervention form
        /// GET:~/SiteEngineer/SiteEngineer/CreateIntervention
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateIntervention()
        {
            var Clients = Engineer.getClients();
            var viewClientsList = new List<SelectListItem>();
            foreach (var client in Clients)
            {
                viewClientsList.Add(new SelectListItem() { Text = client.Name, Value = client.Id.ToString() });
            }

            var InterventionTypes = Engineer.getInterventionTypes();
            var viewInterventionTypes = new List<SelectListItem>();
            foreach (var type in InterventionTypes)
            {
                viewInterventionTypes.Add(new SelectListItem() { Text = type.Name.ToString(), Value = type.Id.ToString() });
            }
            var model = new SiteEngineerViewInterventionModel() { ViewInterventionTypeList = viewInterventionTypes, ViewClientsList = viewClientsList };
            return View(model);
        }


        /// <summary>
        /// Display the edit intervention state form
        /// GET:~/SiteEngineer/SiteEngineer/EditInterventionState/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EditInterventionState(Guid? id)
        {

            if (!id.HasValue)
            {
                return View("Error");
            }

            Intervention intervention = Engineer.getNonGuidInterventionById(id.Value);
            InterventionViewModel model = BindSingleIntervention(intervention);

            return View(model);
        }


        /// <summary>
        /// Edit intervention state
        /// POST:~/SiteEngineer/SiteEngineer/EditInterventionState
        /// </summary>
        /// <param name="interventionmodel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditInterventionState(InterventionViewModel interventionmodel)
        {
            string interventionState = interventionmodel.SelectedState;
            InterventionState interventionStatus = new InterventionState();
            Enum.TryParse(interventionState, out interventionStatus);
            int interventionStateInt = (int)interventionStatus;
            InterventionState newState = (InterventionState)interventionStateInt;

            if (Engineer.updateInterventionState(interventionmodel.Id, newState))
            {
                var interventionList = Engineer.GetAllInterventions(Engineer.getDetail().Id).ToList();
                var interventions = new List<InterventionViewModel>();
                BindIntervention(interventionList, interventions);
                var model = new SiteEngineerViewInterventionModel() { Interventions = interventions };
                return View("InterventionList", model);
            }
            else
            {
                ViewBag.error = "Operation failed, either the state is wrong or you are not authorized";
                Intervention intervention = Engineer.getNonGuidInterventionById(interventionmodel.Id);
                InterventionViewModel model = BindSingleIntervention(intervention);
                return View(model);
            }

        }


        /// <summary>
        /// Create an intervention
        /// POST:~/SiteEngineer/SiteEngineer/CreateIntervention
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateIntervention(SiteEngineerViewInterventionModel viewmodel)
        {
            if (ModelState.IsValid)
            {
                decimal hours = (decimal)viewmodel.Intervention.Hours;
                decimal costs = (decimal)viewmodel.Intervention.Costs;
                int lifeRemaining = 100;
                string comments = viewmodel.Intervention.Comments;
                InterventionState state = InterventionState.Proposed;

                Guid clientId = new Guid(viewmodel.SelectedClient);
                DateTime dateCreate = DateTime.Now;
                DateTime dateFinish = (DateTime)viewmodel.Intervention.DateFinish;
                DateTime dateRecentVisit = DateTime.Now;


                Guid createdBy = (Guid)Engineer.getDetail().Id;
                Guid typeId = new Guid(viewmodel.SelectedType);
                Intervention new_intervention = new Intervention(hours, costs, lifeRemaining, comments, state,
                dateCreate, dateFinish, dateRecentVisit, typeId, clientId, createdBy, null);
                Engineer.createIntervention(new_intervention);

                var interventionList = Engineer.getInterventionsByClient(clientId).ToList();
                var interventions = new List<InterventionViewModel>();
                BindIntervention(interventionList, interventions);

                Client client = Engineer.getClientById(clientId);
                ClientViewModel clientVmodel = new ClientViewModel();
                clientVmodel = BindSingleClient(client);
                var model = new SiteEngineerViewClientModel() { Interventions = interventions, Client = clientVmodel };
                return View("ClientDetails", model);
            }
            return View();
        }

        // GET: SiteEngineer/EditIntervention

        /// <summary>
        /// Display Edit intervention form
        /// GET:~/SiteEngineer/SiteEngineer/EditIntervention/Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EditIntervention(Guid? id)
        {
            if (!id.HasValue)
            {
                return View("Error");
            }

            Intervention intervention = Engineer.getNonGuidInterventionById(id.Value);
            InterventionViewModel model = BindSingleIntervention(intervention);

            return View(model);
        }


        /// <summary>
        /// Edit intervention
        /// POST:~/SiteEngineer/SiteEngineer/EditIntervention
        /// </summary>
        /// <param name="interventionmodel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditIntervention(InterventionViewModel interventionmodel)
        {
            String new_comments = interventionmodel.Comments;
            int new_liferemaining = interventionmodel.LifeRemaining;
            DateTime new_recentvisit = interventionmodel.RecentiVisit;
            if (Engineer.updateInterventionDetail(interventionmodel.Id, new_comments, new_liferemaining, new_recentvisit))
            {
                var intervention = Engineer.getInterventionById(interventionmodel.Id);

                var interventionList = Engineer.getInterventionsByClient(intervention.ClientId.Value);
                List<InterventionViewModel> interventions = new List<InterventionViewModel>();
                BindIntervention(interventionList, interventions);

                Client client = Engineer.getClientById(intervention.ClientId.Value);
                ClientViewModel clientViewModel = BindSingleClient(client);
                var model = new SiteEngineerViewClientModel() { Interventions = interventions, Client = clientViewModel };

                return View("ClientDetails", model);
            }
            else
                return View();

        }



        /// <summary>
        /// Display client details
        /// GET:~/SiteEngineer/SiteEngineer/ClientDetails/Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ClientDetails(Guid? id)
        {
            if (!id.HasValue)
            {
                return View("Error");
            }

            var client = Engineer.getClientById(id.Value);
            var interventionList = Engineer.getInterventionsByClient(id.Value);
            List<InterventionViewModel> interventions = new List<InterventionViewModel>();
            BindIntervention(interventionList, interventions);
            ClientViewModel clientViewModel = BindSingleClient(client);

            InterventionViewModel interview = new InterventionViewModel();
            var model = new SiteEngineerViewClientModel() { Interventions = interventions, Client = clientViewModel, Intervention = interview };
            return View(model);
        }

        // POST: SiteEngineer/InterventionList

        /// <summary>
        ///  Display client details
        ///  POST:~/SiteEngineer/SiteEngineer/ClientDetails
        /// </summary>
        /// <param name="SE_VclientModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ClientDetails(SiteEngineerViewClientModel SE_VclientModel)
        {
            List<InterventionViewModel> ClientList = new List<InterventionViewModel>();
            return View();
        }


        // GET: SiteEngineer/InterventionList
        /// <summary>
        /// Display a list of interventions
        /// GET:~/SiteEngineer/SiteEngineer/InterventionList
        /// </summary>
        /// <returns></returns>
        public ActionResult InterventionList()
        {
            Guid enigerrId = Engineer.getDetail().Id;
            var interventionList = Engineer.GetAllInterventions(Engineer.getDetail().Id).ToList();

            var interventions = new List<InterventionViewModel>();
            BindIntervention(interventionList, interventions);
            var model = new SiteEngineerViewInterventionModel() { Interventions = interventions };
            return View(model);
        }

        // POST: SiteEngineer/InterventionList
        /// <summary>
        /// redirect to edit intervention
        /// POST:~/SiteEngineer/SiteEngineer/InterventionList
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult InterventionList(SiteEngineerViewInterventionModel model)
        {
            InterventionViewModel selectedIntervention = model.Intervention;
            return View("EditIntervention", selectedIntervention);
        }
        /// <summary>
        /// Bind data model with view model
        /// </summary>
        /// <param name="clientList"></param>
        /// <param name="clients"></param>
        public void BindClient(IEnumerable<IMSLogicLayer.Models.Client> clientList, List<ClientViewModel> clients)
        {
            foreach (var client in clientList)
            {
                clients.Add(new ClientViewModel()
                {
                    Id = client.Id,
                    DistrictName = Engineer.getDistrictName(client.DistrictId),
                    Location = client.Location,
                    Name = client.Name
                });
            }
        }
        /// <summary>
        /// Bind data model with view model
        /// </summary>
        /// <param name="interventionList"></param>
        /// <param name="interventions"></param>
        private void BindIntervention(IEnumerable<IMSLogicLayer.Models.Intervention> interventionList, List<InterventionViewModel> interventions)
        {
            foreach (var intervention in interventionList)
            {
                interventions.Add(new InterventionViewModel()
                {
                    InterventionTypeName = intervention.InterventionType.Name,
                    ClientId = (Guid)intervention.ClientId,
                    Id = (Guid)intervention.Id,
                    ClientName = intervention.Client.Name,
                    DateCreate = intervention.DateCreate,
                    InterventionState = intervention.InterventionState.ToString(),
                    LifeRemaining = (int)intervention.LifeRemaining,
                    RecentiVisit = (DateTime)intervention.DateRecentVisit,

                    // ??
                    DistrictName = intervention.District.Name,
                    Costs = intervention.Costs,
                    Hours = intervention.Hours,
                    DateFinish = intervention.DateFinish,
                    Comments = intervention.Comments
                });
            }
        }
        /// <summary>
        /// Bind data model with view model
        /// </summary>
        /// <param name="intervention"></param>
        /// <returns></returns>
        private InterventionViewModel BindSingleIntervention(Intervention intervention)
        {
            var viewInterventionStates = new List<SelectListItem>();
            viewInterventionStates.Add(new SelectListItem() { Text = InterventionState.Approved.ToString(), Value = InterventionState.Approved.ToString() });
            viewInterventionStates.Add(new SelectListItem() { Text = InterventionState.Cancelled.ToString(), Value = InterventionState.Cancelled.ToString() });
            viewInterventionStates.Add(new SelectListItem() { Text = InterventionState.Completed.ToString(), Value = InterventionState.Completed.ToString() });
            viewInterventionStates.Add(new SelectListItem() { Text = InterventionState.Proposed.ToString(), Value = InterventionState.Proposed.ToString() });

            InterventionViewModel interventionmodel = new InterventionViewModel()
            {
                InterventionTypeName = intervention.InterventionType.Name,
                ClientId = (Guid)intervention.ClientId,

                ClientName = intervention.Client.Name,
                DateCreate = intervention.DateCreate,
                InterventionState = intervention.InterventionState.ToString(),
                LifeRemaining = (int)intervention.LifeRemaining,
                RecentiVisit = (DateTime)intervention.DateRecentVisit,

                // ??
                DistrictName = intervention.District.Name,
                Costs = intervention.Costs,
                Hours = intervention.Hours,
                DateFinish = intervention.DateFinish,
                Comments = intervention.Comments,
                InterventionStates = viewInterventionStates
            };
            return interventionmodel;
        }
        /// <summary>
        /// Bind data model with view model
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private ClientViewModel BindSingleClient(Client client)
        {
            ClientViewModel clientmodel = new ClientViewModel()
            {
                Id = client.Id,
                DistrictName = Engineer.getDistrictName(client.DistrictId),
                Location = client.Location,
                Name = client.Name
            };
            return clientmodel;
        }
    }
}