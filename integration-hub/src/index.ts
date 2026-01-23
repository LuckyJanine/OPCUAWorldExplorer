import http from 'http';
import express from "express"; 
import * as RED from 'node-red';
import dispatchRoutes from "./routes/dispatch";

const app = express();            // uses `body-parser` under the hood
// `body-parser`:
// 1. Reads the incoming request body as a stream
// 2. Converts the stream of bytes into a string
// 3. Parses the string using `JSON.parse()`
// 4. Attaches the resulting object to `req.body`
// parsing errors -> 400 

const PORT = 3000;

const server = http.createServer(app);

const redsettings: any = {
  httpAdminRoot: '/red',          //    ` .../red `                  Node-RED editor - Mount path for Node-RED editor UI
  httpNodeRoot: '/api-red',       //    ` .../api-red/* `            Node-RED HTTP nodes - Mount path for Node-RED HTTP nodes (e.g., http in nodes)
  userDir: './.nodered',          //                                 Node-RED config & flows folder - Directory to store Node-RED flows, credentials, settings, and node modules
  functionGlobalContext: {},      //                                 Optional: share services with Node-RED - Objects/functions exposed to Node-RED Function nodes via global.get('key')
  flowFile: 'flows.json'
};

RED.init(server, redsettings);    // Node-RED itself is a Node.js runtime that exposes an HTTP API for flows and dashboard interactions.
// 

app.use(redsettings.httpAdminRoot, RED.httpAdmin); 
app.use(redsettings.httpNodeRoot, RED.httpNode);

// app.use(express.json());          // register middleware: built-in middleware in Express
// to look at incoming HTTP requests 
// If the request has a `Content-Type` of `application/json`, it will:
//      - Parse the JSON string in the request body into a JavaScript object.
//      - Attach that object to `req.body`
// without it, if json data, `req.body` will be `undefined`

app.use("/api", dispatchRoutes);  //   ` .../api/* `               Express http endpoints

// start the server and then the callback
server.listen(PORT, async () => {
  console.log(`http server running at http://localhost:${PORT}/api\n`);
  await RED.start();
  console.log(`node-RED editor at http://localhost:${PORT}/red`);
});