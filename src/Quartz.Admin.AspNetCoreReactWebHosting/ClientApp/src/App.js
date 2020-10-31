import React from "react";
import { Route } from "react-router";
import { Switch } from "react-router-dom";
import { Layout } from "./components/Layout";
import { Dashboard } from "./components/Dashboard";

import "./custom.css";

export default function App() {
  return (
    <Layout>
      <Switch>
        {/* <Route exact path='/' component={Home} /> */}
        <Route path="/" component={Dashboard} />
        <Route component={() => <p>Not Found!</p>} />
      </Switch>
    </Layout>
  );
}
