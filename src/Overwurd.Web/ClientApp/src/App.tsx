import React from 'react';
import { Route, Routes } from 'react-router-dom';
import { Layout } from './components/Layout';
import { Home } from './components/Home';

export const App = () =>
  <Layout>
    <Routes>
      <Route path='/' element={<Home />} />
    </Routes>
  </Layout>;