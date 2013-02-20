using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using SqlTrigger.Data;

namespace SqlTrigger.Web.Controllers
{
    public class TestModel
    {
        public string Value { get; set; }
    }

    public class FishTypesController : ApiController
    {
        // GET api/values
        public FishType[] Get()
        {
            var repo = new FishRepository();
            return repo.GetFishTypes();
        }
    }

    public class FishController : ApiController
    {
        // GET api/values
        public FishDto[] Get()
        {
            var repo = new FishRepository();
            return repo.GetFish();
        }
    }

    public class ValuesController : ApiController
    {
        // GET api/values
        public void Get()
        {
             
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody] string value)
        {

        }

        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }


    public class FishHub : Hub
    {
        public void Add(Fish fish)
        {
            var repo = new FishRepository();
            repo.AddFish(fish);
        }
    }
}
