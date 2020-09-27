import React, { Component, useEffect, useRef, useState } from "react";
import {
  Button,
  Form,
  FormGroup,
  Input,
  Label,
  Modal,
  ModalBody,
  ModalFooter,
  ModalHeader,
  Table,
} from "reactstrap";

const DashboardContext = React.createContext({});

export function Dashboard() {
  const [show, setShow] = useState(false);
  const [job, setJob] = useState({
    id: undefined,
    jobName: "a",
    jobGroup: "g",
  });
  const alertRef = useRef(null);

  return (
    <div>
      <Button color="info" onClick={() => alertRef.current.toggleShow()}>
        New Job
      </Button>
      <JobsTable
        onEdit={(item) => {
          console.log(item);
          alertRef.current.setState({
            setting: item,
          });
          alertRef.current.toggleShow();
        }}
      />
      <MyAlertModal ref={alertRef} />
    </div>
  );
}

const testData = [
  {
    id: 1,
    jobName: "job1",
    jobGroup: "group1",
    jobDesc: "desc1",
    selected: false,
  },
  {
    id: 2,
    jobName: "job2",
    jobGroup: "group2",
    jobDesc: "desc2",
    selected: false,
  },
  {
    id: 3,
    jobName: "job3",
    jobGroup: "group3",
    jobDesc: "desc3",
    selected: false,
  },
  {
    id: 4,
    jobName: "job4",
    jobGroup: "group4",
    jobDesc: "desc4",
    selected: false,
  },
];

function JobsTable(props) {
  const [loading, setLoading] = useState(true);
  const [jobs, setJobs] = useState([]);
  const [selectedAll, setSelectedAll] = useState(false);

  useEffect(() => {
    setTimeout(() => {
      // TODO: fetch jobs from api
      setJobs(testData);
      setLoading(false);
    }, 1000);
  }, []);

  return (
    <div className="mt-3">
      {loading ? (
        <i>Loading ...</i>
      ) : jobs.length ? (
        <Table striped responsive>
          <thead>
            <tr>
              <th className="">
                <FormGroup check>
                  <Label check>
                    <Input
                      type="checkbox"
                      id="selectAll"
                      checked={selectedAll}
                      onChange={(event) => {
                        setSelectedAll(!selectedAll);
                        setJobs(
                          jobs.map((i) => {
                            i.selected = event.target.checked;
                            return i;
                          })
                        );
                      }}
                    />
                    All
                  </Label>
                </FormGroup>
              </th>
              <th>Job Name</th>
              <th>Job Group</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {jobs.map((item, idx) => (
              <tr key={idx}>
                <th scope="row">
                  <FormGroup check>
                    <Label check>
                      <Input
                        type="checkbox"
                        id={"item" + item.id}
                        checked={item.selected}
                        value={idx}
                        onChange={(event) => {
                          const newJobs = jobs.slice();
                          newJobs[idx].selected = event.target.checked;
                          setJobs(newJobs);
                          if (selectedAll) setSelectedAll(false);
                        }}
                      />
                      #{item.id}
                    </Label>
                  </FormGroup>
                </th>
                <td>{item.jobName}</td>
                <td>{item.jobGroup}</td>
                <td>
                  <Button
                    size="sm"
                    color="link"
                    className="mb-2 mb-md-0"
                    onClick={() => {}}
                  >
                    Logs
                  </Button>{" "}
                  <Button
                    size="sm"
                    color="primary"
                    className="mb-2 mb-md-0"
                    onClick={() => {
                      props.onEdit(item);
                    }}
                  >
                    Edit
                  </Button>{" "}
                  <Button
                    size="sm"
                    color="warning"
                    className="mb-2 mb-md-0"
                    onClick={() => {}}
                  >
                    Stop
                  </Button>{" "}
                  <Button
                    size="sm"
                    color="success"
                    className="mb-2 mb-md-0"
                    onClick={() => {}}
                  >
                    Start
                  </Button>
                </td>
              </tr>
            ))}
          </tbody>
        </Table>
      ) : (
        <p>No jobs, please click button to create a one!</p>
      )}
    </div>
  );
}

const initSetting = () => ({
  id: undefined,
  jobName: "",
  jobGroup: "",
  jobDesc: "",
  triggerType: "",
  triggerValue: "",
  // TODO: others properties
});

class MyAlertModal extends Component {
  constructor(props, ref) {
    super(props);

    this.state = {
      show: false,
      setting: initSetting(),
    };

    this.toggleShow = this.toggleShow.bind(this);
    this.toggleChange = this.toggleChange.bind(this);
  }

  toggleShow() {
    if (this.state.show) {
      this.setState({
        setting: initSetting(),
      });
    }
    this.setState({
      show: !this.state.show,
    });
  }

  toggleChange(event) {
    const target = event.target;
    const value = target.type === "checkbox" ? target.checked : target.value;
    const name = target.name;

    const newSetting = this.state.setting;
    newSetting[name] = value;
    this.setState({
      setting: newSetting,
    });
  }

  render() {
    return (
      <Modal isOpen={this.state.show} toggle={this.toggleShow}>
        <ModalHeader>Setting</ModalHeader>
        <ModalBody>
          <Form>
            <FormGroup>
              <Input
                placeholder="Job name"
                name="jobName"
                value={this.state.setting.jobName}
                onChange={this.toggleChange}
                required
              />
            </FormGroup>
            <FormGroup>
              <Input
                placeholder="Job Group"
                name="jobGroup"
                value={this.state.setting.jobGroup}
                onChange={this.toggleChange}
                required
              />
            </FormGroup>
            <FormGroup>
              <Input
                placeholder="Job Description"
                name="jobDesc"
                value={this.state.setting.jobDesc}
                onChange={this.toggleChange}
                required
              />
            </FormGroup>
            <FormGroup>
              <Input
                placeholder="Trigger Type"
                name="triggerType"
                type="select"
                value={this.state.setting.triggerType || 0}
                onChange={this.toggleChange}
                required
              >
                <option value={0}>Simple Trigger</option>
                <option value={1}>Cron Trigger</option>
              </Input>
            </FormGroup>
            <FormGroup>
              <Input
                placeholder="Trigger Value"
                name="triggerValue"
                value={this.state.setting.triggerValue || ""}
                onChange={this.toggleChange}
                required
              />
            </FormGroup>
          </Form>
        </ModalBody>
        <ModalFooter>
          <Button onClick={this.toggleShow} color="primary">
            Submit
          </Button>
          <Button onClick={this.toggleShow}>Cancel</Button>
        </ModalFooter>
      </Modal>
    );
  }
}
