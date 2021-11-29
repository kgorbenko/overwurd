import React, { Component } from 'react';
import { Link } from 'react-router-dom';

export class NavMenu extends Component {
  static displayName = NavMenu.name;

  constructor (props) {
    super(props);

    this.toggleNavbar = this.toggleNavbar.bind(this);
    this.state = {
      collapsed: true
    };
  }

  toggleNavbar () {
    this.setState({
      collapsed: !this.state.collapsed
    });
  }

  render () {
    return (
      <header>
        <Link to="/">Home</Link>
        <Link to="/counter">Counter</Link>
        <Link to="/fetch-data">Fetch data</Link>
      </header>
    );
  }
}
