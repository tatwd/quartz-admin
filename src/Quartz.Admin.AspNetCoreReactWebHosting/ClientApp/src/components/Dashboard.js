import React, {
  Component,
  forwardRef,
  useContext,
  useEffect,
  useRef,
  useState,
} from "react";
import {
  Alert,
  Button,
  Form,
  FormGroup,
  Input,
  Modal,
  ModalBody,
  ModalFooter,
  ModalHeader,
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

  const myContext = {
    show,
    toggleShow: () => setShow(!show),
    job,
    updateJob: (key, val) => {
      var newJob = job;
      newJob[key] = val;
      console.log(newJob);
      setJob(newJob);
    },
  };

  return (
    <DashboardContext.Provider value={myContext}>
      {/* <EditPannal /> */}
      {/* <JobTab /> */}
      <MyAlertModal ref={alertRef} />
      <Button onClick={() => alertRef.current.toggleShow()}>New Job</Button>
    </DashboardContext.Provider>
  );
}

function EditPannal() {
  const { show, toggleShow, job, updateJob } = useContext(DashboardContext);
  const toggleChange = (event) => {
    updateJob(event.target.name, event.target.value);
  };
  return show ? (
    <Form>
      <FormGroup>
        <Input value={job.jobName} name="jobName" onChange={toggleChange} />
      </FormGroup>
      <FormGroup>
        <Input value={job.jobGroup} name="jobGroup" onChange={toggleChange} />
      </FormGroup>
      <Button onClick={toggleShow}>toggleShow</Button>
    </Form>
  ) : (
    ""
  );
}

function JobTab() {
  const { toggleShow, updateJob } = useContext(DashboardContext);
  const [loading, setLoading] = useState(true);
  const [jobs, setJobs] = useState([1]);

  useEffect(() => {
    setTimeout(() => {
      console.log("fired");
      setJobs([1, 2, 3, 4]);
      setLoading(false);
    }, 1000);
  }, []);

  return (
    <div>
      <Button color="primary" onClick={toggleShow}>
        toggleShow
      </Button>
      {loading ? (
        <p>
          <i>Loading ...</i>
        </p>
      ) : (
        <ul>
          {jobs.map((i) => (
            <li key={i}>
              {i}
              <Button
                size="sm"
                onClick={() => {
                  updateJob("jobName", "job" + i);
                }}
              >
                toggle
              </Button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

class MyAlertModal extends Component {
  constructor(props, ref) {
    super(props);
    this.state = {
      hide: false,
      setting: {
        jobName: "",
      },
    };

    this.toggleShow = this.toggleShow.bind(this);
    this.toggleChange = this.toggleChange.bind(this);
  }

  toggleShow() {
    this.setState({
      hide: !this.state.hide,
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
      <Modal isOpen={this.state.hide} toggle={this.toggleShow}>
        <ModalHeader>Setting</ModalHeader>
        <ModalBody>
          <Form>
            <FormGroup>
              <Input
                placeholder="Job name"
                name="jobName"
                value={this.state.setting.jobName}
                onClick={this.toggleChange}
              />
            </FormGroup>
            <FormGroup>
              <Input
                placeholder="Job Group"
                name="jobGrop"
                value={this.state.setting.jobGrop}
                onClick={this.toggleChange}
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
