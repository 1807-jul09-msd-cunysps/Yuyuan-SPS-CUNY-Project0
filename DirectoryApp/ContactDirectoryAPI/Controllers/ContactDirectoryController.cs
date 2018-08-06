using DirectoryDAL;
using ContactDirectoryLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web;
using Newtonsoft.Json.Converters;

namespace ContactDirectoryAPI.Controllers
{
    //[EnableCors("*", "*", "*")]
    public class ContactDirectoryController : ApiController
    {
        private DbHandler db = new DbHandler();

        [Route("contactdir/get/all")]
        [HttpGet]
        public IHttpActionResult Get()
        {
            return Json(db.LoadEntries(), new JsonSerializerSettings { Formatting = Formatting.Indented }, Encoding.UTF8);
        }

        [Route("contactdir/get/{id}")]
        [HttpGet]
        public IHttpActionResult Get(long id)
        {
            return Json(db.ReadPerson(id), new JsonSerializerSettings { Formatting = Formatting.Indented }, Encoding.UTF8);
        }

        [Route("contactdir/post/joinme")]
        [HttpPost]
        public IHttpActionResult Post([FromBody]Person p)
        {
            try
            {
                db.AddPersonWithoutId(ref p);
                return Ok("Successful");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("contactdir/post/contactme")]
        [HttpPost]
        public IHttpActionResult Post([FromBody]DbHandler.ContactMeMessage m) // POST: ContactDirectory/Post
        {
            try
            {
                db.AddMessage(m);
                return Ok("Successful");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
