import React, { Component } from 'react';
import { Route, Routes } from 'react-router-dom';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { Counter } from './components/Counter';

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
        <Routes>
          <Route exact path='/' element={<Home />} />
          <Route path='/counter' element={<Counter />} />
        </Routes>
      </Layout>
    );
  }
}
