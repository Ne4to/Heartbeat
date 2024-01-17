import React, { Component } from 'react';
import { Collapse, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import './NavMenu.css';

export class NavMenu extends Component {
  static displayName = NavMenu.name;

  constructor(props) {
    super(props);

    this.toggleNavbar = this.toggleNavbar.bind(this);
    this.state = {
      collapsed: true
    };
  }

  toggleNavbar() {
    this.setState({
      collapsed: !this.state.collapsed
    });
  }

  render() {
    return (
      <header>
        {/* TODO try https://mui.com/material-ui/react-app-bar/ */}
        <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3" container light>
          <NavbarBrand tag={Link} to="/">Heartbeat</NavbarBrand>
          <NavbarToggler onClick={this.toggleNavbar} className="mr-2" />
          <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!this.state.collapsed} navbar>
            <ul className="navbar-nav flex-grow">
              {/* <NavItem>
                <NavLink tag={Link} className="text-dark" to="/">Home</NavLink>
              </NavItem> */}
              <NavItem>
                {/* <NavLink tag={Link} className="text-dark" to="https://github.com/Ne4to/Heartbeat" target="_blank">About</NavLink> */}
                <a href="https://github.com/Ne4to/Heartbeat" target="_blank">About</a>
              </NavItem>
              {/* <NavItem>
                <NavLink tag={Link} className="text-dark" to="/modules">Modules</NavLink>
              </NavItem>
              <NavItem>
                <NavLink tag={Link} className="text-dark" to="/instance-type-statistics">Instance type statistics</NavLink>
              </NavItem> */}
            </ul>
          </Collapse>
        </Navbar>
      </header>
    );
  }
}
